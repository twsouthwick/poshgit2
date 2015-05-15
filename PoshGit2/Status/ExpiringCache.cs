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
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly Func<string, ICurrentWorkingDirectory, IRepositoryStatus> _factory;
        private readonly ILogger _log;

        public ExpiringCache(ILogger log, Func<string, ICurrentWorkingDirectory, IRepositoryStatus> factory)
        {
            _log = log;
            _factory = factory;
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

        private static string FindGitRepo(string path)
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
                var up = Path.GetDirectoryName(path);

                return string.Equals(up, path, StringComparison.OrdinalIgnoreCase) ? null : FindGitRepo(up);
            }
        }

        public Task RemoveRepoAsync(string path, CancellationToken cancellationToken)
        {
            Remove(path);

            return Task.FromResult(false);
        }

        private void Remove(string path)
        {
            var mainPath = FindGitRepo(path);

            if (mainPath == null)
            {
                return;
            }

            _cache.Remove(mainPath);
        }

        public void Dispose()
        {
            _cache.Dispose();
        }
    }
}
