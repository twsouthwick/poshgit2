using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public sealed class ExpiringCache : IDisposable, IRepositoryCache
    {
        private readonly Func<string, ICurrentWorkingDirectory, IRepositoryStatus> _factory;
        private readonly ILogger _log;
        private readonly IFormatStatusString _writer;

        // This is replaced when it is cleared
        private MemoryCache _cache;

        public ExpiringCache(ILogger log, Func<string, ICurrentWorkingDirectory, IRepositoryStatus> factory, IFormatStatusString writer)
        {
            _log = log;
            _writer = writer;
            _factory = factory;

            _cache = GetCache();
        }

        private static MemoryCache GetCache()
        {
            return new MemoryCache("PoshGit2");
        }

        public Task<IEnumerable<IRepositoryStatus>> GetAllReposAsync(CancellationToken cancellationToken)
        {
            var all = _cache
                .Select(o => new ReadonlyCopyRepositoryStatus(o.Value as IRepositoryStatus))
                .ToList();

            return Task.FromResult<IEnumerable<IRepositoryStatus>>(all);
        }

        public Task<IRepositoryStatus> FindRepoAsync(ICurrentWorkingDirectory cwd, CancellationToken cancellationToken)
        {
            var @null = Task.FromResult<IRepositoryStatus>(null);

            if (!cwd.IsValid)
            {
                return @null;
            }

            var path = cwd.CWD;
            var repo = FindGitRepo(path);

            if (repo == null)
            {
                return @null;
            }

            lock (_cache)
            {
                if (_cache.Contains(repo))
                {
                    var item = _cache.GetCacheItem(repo);
                    var value = item.Value as IRepositoryStatus;

                    if (value == null)
                    {
                        _log.Warning("Found an entry that is not IRepositoryStatus: {CacheValue}", item.Value?.GetType());
                        _cache.Remove(repo);
                    }
                    else
                    {
                        _log.Verbose("Found repo: {Path}", repo);
                        return Task.FromResult(new ReadonlyCopyRepositoryStatus(value, cwd) as IRepositoryStatus);
                    }
                }

                try
                {
                    _log.Information("Creating repo: {Path}", repo);

                    var status = _factory(repo, cwd);
                    var policy = new CacheItemPolicy
                    {
                        RemovedCallback = arg =>
                        {
                            _log.Information("Removing repo from cache: {Repo}", repo);
                            (arg.CacheItem.Value as IDisposable)?.Dispose();
                        },
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    };

                    _cache.Add(new CacheItem(repo, status), policy);

                    return Task.FromResult(new ReadonlyCopyRepositoryStatus(status, cwd) as IRepositoryStatus);
                }
                catch (RepositoryNotFoundException)
                {
                    return @null;
                }
                catch (Exception e)
                {
                    _log.Warning(e, "Unknown exception in ExpiringCache");
                    return @null;
                }
            }
        }

        public async Task<string> GetStatusStringAsync(string formatString, ICurrentWorkingDirectory cwd, CancellationToken token)
        {
            var status = await FindRepoAsync(cwd, token);

            return _writer.Format(formatString, status);
        }

        private string FindGitRepo(string path)
        {
            if (path == null)
            {
                return null;
            }

            if (Directory.Exists(Path.Combine(path, ".git")))
            {
                return path;
            }
            else
            {
                try
                {
                    var up = Path.GetDirectoryName(path);

                    return string.Equals(up, path, StringComparison.OrdinalIgnoreCase) ? null : FindGitRepo(up);
                }
                catch (Exception e)
                {
                    _log.Error("Invalid repo path: {Path}, {Exception}", path, e);

                    return null;
                }
            }
        }

        public Task<bool> RemoveRepoAsync(string path, CancellationToken cancellationToken)
        {
            return Task.FromResult(Remove(path));
        }

        public Task<bool> ClearCacheAsync(CancellationToken cancellationToken)
        {
            var original = Interlocked.Exchange(ref _cache, GetCache());

            original.Dispose();

            return Task.FromResult(true);
        }

        private bool Remove(string path)
        {
            var mainPath = FindGitRepo(path);

            if (mainPath == null)
            {
                return false;
            }

            var removed = _cache.Remove(mainPath);

            return removed != null;
        }

        public void Dispose()
        {
            _cache.Dispose();
        }
    }
}
