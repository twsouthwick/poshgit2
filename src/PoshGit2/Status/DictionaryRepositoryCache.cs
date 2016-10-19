using System;
using System.Collections.Generic;
using System.Linq;

namespace PoshGit2.Status
{
    public sealed class DictionaryRepositoryCache : RepositoryCache
    {
        private readonly Dictionary<string, IRepositoryStatus> _repositories = new Dictionary<string, IRepositoryStatus>(StringComparer.OrdinalIgnoreCase);

        public DictionaryRepositoryCache(ILogger log, Func<string, ICurrentWorkingDirectory, IRepositoryStatus> factory)
            : base(log, factory)
        {
        }

        protected override IEnumerable<IRepositoryStatus> Repositories => _repositories.Values;

        protected override IRepositoryStatus FindRepo(string repo, ICurrentWorkingDirectory cwd)
        {
            IRepositoryStatus oldStatus;
            if (_repositories.TryGetValue(repo, out oldStatus))
            {
                Log.Information("Found repo: {Path}", repo);

                return new ReadonlyCopyRepositoryStatus(oldStatus, cwd);
            }


            Log.Information("Creating repo: {Path}", repo);

            var status = RepositoryFactory(repo, cwd);

            _repositories.Add(repo, status);

            return new ReadonlyCopyRepositoryStatus(status, cwd);
        }

        protected override void Clear()
        {
            _repositories.Clear();
        }

        protected override bool Remove(string path)
        {
            IRepositoryStatus status;
            if (_repositories.TryGetValue(path, out status))
            {
                (status as IDisposable)?.Dispose();
            }

            return _repositories.Remove(path);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                var all = _repositories.Keys.ToList();

                foreach (var repo in all)
                {
                    RemovePath(repo);
                }
            }
        }
    }
}
