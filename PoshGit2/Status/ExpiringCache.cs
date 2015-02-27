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

        public ExpiringCache(Func<string, IRepositoryStatus> factory)
        {
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
                        Trace.WriteLine($"Found an entry that is not IRepositoryStatus");
                        _cache.Remove(repo);
                    }
                    else
                    {
                        Trace.WriteLine($"Found repo for {repo}");
                        return new ReadonlyCopyRepositoryStatus(value, cwd);
                    }
                }

                try
                {
                    Trace.WriteLine($"Creating new repo for {repo}");

                    var status = _factory(repo);
                    var policy = new CacheItemPolicy
                    {
                        RemovedCallback = arg =>
                        {
                            Trace.WriteLine($"Removing {repo} from cache");
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
                    Trace.WriteLine(e);
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

                if (String.Equals(up, path, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                else
                {
                    return FindGitRepo(up);
                }
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
