using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public static class ServerInfo
    {
        public static string Name { get; } = $"PoshGit2_Server{ServerVersion}_Library{LibraryVersion}";

        private static Version LibraryVersion { get; } = typeof(NamedPipeCommand).GetTypeInfo().Assembly.GetName().Version;

        private static Version ServerVersion { get; } = typeof(ServerStartupRepoCache).GetTypeInfo().Assembly.GetName().Version;
    }

    public class ServerStartupRepoCache : IRepositoryCache
    {
        private readonly ILogger _log;
        private readonly IRepositoryCache _other;
        private readonly bool _showServer;

        public ServerStartupRepoCache(IRepositoryCache other, ILogger log, bool showServer)
        {
            _other = other;
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

            return _other.FindRepoAsync(cwd, cancellationToken);
        }

        public Task<IEnumerable<IRepositoryStatus>> GetAllReposAsync(CancellationToken cancellationToken)
        {
            EnsureServerIsAvailable();

            return _other.GetAllReposAsync(cancellationToken);
        }

        public Task<bool> RemoveRepoAsync(string path, CancellationToken cancellationToken)
        {
            EnsureServerIsAvailable();

            return _other.RemoveRepoAsync(path, cancellationToken);
        }

        public async Task<bool> ClearCacheAsync(CancellationToken token)
        {
            EnsureServerIsAvailable();

            return await _other.ClearCacheAsync(token);
        }
    }
}
