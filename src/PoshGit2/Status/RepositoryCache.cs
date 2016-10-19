using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public abstract class RepositoryCache : IDisposable, IRepositoryCache
    {
        private static readonly Task<IRepositoryStatus> s_null = Task.FromResult<IRepositoryStatus>(null);

        public RepositoryCache(ILogger log, Func<string, ICurrentWorkingDirectory, IRepositoryStatus> factory)
        {
            Log = log;
            RepositoryFactory = factory;
        }

        protected abstract IEnumerable<IRepositoryStatus> Repositories { get; }

        protected Func<string, ICurrentWorkingDirectory, IRepositoryStatus> RepositoryFactory { get; }

        protected ILogger Log { get; }

        public Task<IEnumerable<IRepositoryStatus>> GetAllReposAsync(CancellationToken cancellationToken)
        {
            var all = Repositories
                .Select(o => new ReadonlyCopyRepositoryStatus(o))
                .ToList();

            return Task.FromResult<IEnumerable<IRepositoryStatus>>(all);
        }

        public Task<IRepositoryStatus> FindRepoAsync(ICurrentWorkingDirectory cwd, CancellationToken cancellationToken)
        {
            if (!cwd.IsValid)
            {
                return s_null;
            }

            var path = cwd.CWD;
            var repo = FindGitRepo(path);

            if (repo == null)
            {
                return s_null;
            }

            try
            {
                var result = FindRepo(repo, cwd);

                return Task.FromResult(result);
            }
            catch (RepositoryNotFoundException)
            {
                return s_null;
            }
            catch (Exception e)
            {
                Log.Warning(e, "Unknown exception in cache");
                return s_null;
            }
        }

        public async Task<string> GetStatusStringAsync(IGitPromptSettings settings, ICurrentWorkingDirectory cwd, CancellationToken token)
        {
            var status = await FindRepoAsync(cwd, token);
            var vt100 = new VT100StatusWriter(settings);

            vt100.WriteStatus(status);

            return vt100.Status;
        }

        public Task<bool> RemoveRepoAsync(string path, CancellationToken cancellationToken)
        {
            var repoPath = FindGitRepo(path);

            return Task.FromResult(RemovePath(repoPath));
        }

        public Task<bool> ClearCacheAsync(CancellationToken cancellationToken)
        {
            Clear();
            return Task.FromResult(true);
        }

        protected abstract IRepositoryStatus FindRepo(string repo, ICurrentWorkingDirectory cwd);

        protected abstract void Clear();

        protected abstract bool Remove(string path);

        protected bool RemovePath(string path)
        {
            var mainPath = FindGitRepo(path);

            if (mainPath == null)
            {
                return false;
            }

            return Remove(mainPath);
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

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
    }
}
