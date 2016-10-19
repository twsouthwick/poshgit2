using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;

namespace PoshGit2
{
    public sealed class ExpiringCache : RepositoryCache
    {
        // This is replaced when it is cleared
        private MemoryCache _cache;

        public ExpiringCache(ILogger log, Func<string, ICurrentWorkingDirectory, IRepositoryStatus> factory, IStatusWriterProvider writerProvider)
            : base(log, factory, writerProvider)
        {
            _cache = GetCache();
        }

        private static MemoryCache GetCache()
        {
            return new MemoryCache("PoshGit2");
        }

        protected override IEnumerable<IRepositoryStatus> Repositories => _cache.Select(r => r.Value as IRepositoryStatus);

        protected override IRepositoryStatus FindRepo(string repo, ICurrentWorkingDirectory cwd)
        {
            lock (_cache)
            {
                if (_cache.Contains(repo))
                {
                    var item = _cache.GetCacheItem(repo);
                    var value = item.Value as IRepositoryStatus;

                    if (value == null)
                    {
                        Log.Warning("Found an entry that is not IRepositoryStatus: {CacheValue}", item.Value?.GetType());
                        _cache.Remove(repo);
                    }
                    else
                    {
                        Log.Verbose("Found repo: {Path}", repo);
                        return new ReadonlyCopyRepositoryStatus(value, cwd);
                    }
                }

                Log.Information("Creating repo: {Path}", repo);

                var status = RepositoryFactory(repo, cwd);
                var policy = new CacheItemPolicy
                {
                    RemovedCallback = arg =>
                    {
                        Log.Information("Removing repo from cache: {Repo}", repo);
                        (arg.CacheItem.Value as IDisposable)?.Dispose();
                    },
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                };

                _cache.Add(new CacheItem(repo, status), policy);

                return new ReadonlyCopyRepositoryStatus(status, cwd);
            }
        }

        protected override void Clear()
        {
            var original = Interlocked.Exchange(ref _cache, GetCache());

            original.Dispose();
        }

        protected override bool Remove(string path)
        {
            var removed = _cache.Remove(path);

            return removed != null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cache.Dispose();
                _cache = null;
            }
        }
    }
}
