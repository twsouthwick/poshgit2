using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public class ServerStartupPoshGitClient : IRepositoryCache, ITabCompleter
    {
        private readonly ILogger _log;
        private readonly IRepositoryCache _repositoryCache;
        private readonly ITabCompleter _tabCompleter;
        private readonly bool _showServer;

        public ServerStartupPoshGitClient(IRepositoryCache repositoryCache, ITabCompleter tabCompleter, ILogger log, bool showServer)
        {
            _repositoryCache = repositoryCache;
            _tabCompleter = tabCompleter;
            _log = log;
            _showServer = showServer;
        }

        public async void EnsureServerIsAvailable()
        {
            await Task.Run(() =>
            {
                // Check to see if the server is running
                bool createdNewServerMutex;
                using (var serverMutex = new Mutex(true, ServerInfo.Name, out createdNewServerMutex))
                { }

                if (!createdNewServerMutex)
                {
                    return;
                }

                // If mutex was created, server is not running. Launch process
                var location = Path.GetDirectoryName(this.GetType().Assembly.Location);

                var psi = new ProcessStartInfo
                {
                    FileName = "PoshGit2.Server.exe",
                    WorkingDirectory = location,
                    WindowStyle = _showServer ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden
                };

                _log.Debug("Launching server as it is not running: {@ProcessStartInfo}", psi);

                try
                {
                    using (Process.Start(psi)) { }
                }
                catch (Exception e)
                {
                    _log.Error(e, "Could not start server");
                }
            });
        }

        public Task<IRepositoryStatus> FindRepoAsync(ICurrentWorkingDirectory cwd, CancellationToken cancellationToken)
        {
            EnsureServerIsAvailable();

            return _repositoryCache.FindRepoAsync(cwd, cancellationToken);
        }

        public Task<string> GetStatusStringAsync(string statusString, ICurrentWorkingDirectory cwd, CancellationToken cancellationToken)
        {
            EnsureServerIsAvailable();

            return _repositoryCache.GetStatusStringAsync(statusString, cwd, cancellationToken);
        }

        public Task<IEnumerable<IRepositoryStatus>> GetAllReposAsync(CancellationToken cancellationToken)
        {
            EnsureServerIsAvailable();

            return _repositoryCache.GetAllReposAsync(cancellationToken);
        }

        public Task<bool> RemoveRepoAsync(string path, CancellationToken cancellationToken)
        {
            EnsureServerIsAvailable();

            return _repositoryCache.RemoveRepoAsync(path, cancellationToken);
        }

        public Task<bool> ClearCacheAsync(CancellationToken token)
        {
            EnsureServerIsAvailable();

            return _repositoryCache.ClearCacheAsync(token);
        }

        public Task<TabCompletionResult> CompleteAsync(string line, CancellationToken token)
        {
            EnsureServerIsAvailable();

            return _tabCompleter.CompleteAsync(line, token);
        }
    }
}
