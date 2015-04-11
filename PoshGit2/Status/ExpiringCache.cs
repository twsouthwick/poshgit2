using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Caching;

namespace PoshGit2
{
    public sealed class ExpiringCache : IDisposable, IRepositoryCache
    {
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly Func<string, IRepositoryStatus> _factory;
        private readonly ILogger _log;

        public ExpiringCache(ILogger log, Func<string, IRepositoryStatus> factory)
        {
            _log = log;
            _factory = factory;
        }

        public IEnumerable<IRepositoryStatus> All
        {
            get
            {
                return _cache.Select(o => new ReadonlyCopyRepositoryStatus(o.Value as IRepositoryStatus)).ToList();
            }
        }

        public IRepositoryStatus FindRepo(ICurrentWorkingDirectory cwd)
        {
            if (!cwd.IsValid)
            {
                return null;
            }

            var path = cwd.CWD;
            var repo = FindGitRepo(path);

            if (repo == null)
            {
                return null;
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
                        _log.Information("Found repo: {Path}", repo);
                        return new ReadonlyCopyRepositoryStatus(value, cwd);
                    }
                }

                try
                {
                    _log.Information("Creating repo: {Path}", repo);

                    var status = _factory(repo);
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

                    return new ReadonlyCopyRepositoryStatus(status, cwd);
                }
                catch (RepositoryNotFoundException)
                {
                    return null;
                }
                catch (Exception e)
                {
                    _log.Warning(e, "Unknown exception in ExpiringCache");
                    return null;
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

        public void Remove(IRepositoryStatus repository)
        {
            Remove(repository.GitDir);
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
