using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public class DaemonStartupRepoCache : IRepositoryCache
    {
        private readonly ILogger _log;
        private readonly IRepositoryCache _other;
        private readonly bool _showServer;

        public DaemonStartupRepoCache(IRepositoryCache other, ILogger log, bool showServer)
        {
            _other = other;
            _log = log;
            _showServer = showServer;
        }

        public async void EnsureServerIsAvailable()
        {
            await Task.Run(() =>
            {
                var location = Path.GetDirectoryName(this.GetType().Assembly.Location);

                _log.Information("Launching daemon in case it hasn't been started from {WorkingDirectory}", location);

                var psi = new ProcessStartInfo
                {
                    FileName = "PoshGit2.Daemon.exe",
                    WorkingDirectory = location,
                    WindowStyle = _showServer ? ProcessWindowStyle.Normal: ProcessWindowStyle.Hidden
                };

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

            return _other.FindRepoAsync(cwd, cancellationToken);
        }

        public Task<IEnumerable<IRepositoryStatus>> GetAllReposAsync(CancellationToken cancellationToken)
        {
            EnsureServerIsAvailable();

            return _other.GetAllReposAsync(cancellationToken);
        }

        public Task RemoveRepoAsync(string path, CancellationToken cancellationToken)
        {
            EnsureServerIsAvailable();

            return _other.RemoveRepoAsync(path, cancellationToken);
        }
    }
}
