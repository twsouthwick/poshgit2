using Autofac;
using PoshGit2.IO;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public sealed class RepoCacheTestLoop : IDisposable
    {
        private readonly IRepositoryCache _cache;
        private readonly ILogger _log;
        private readonly ILifetimeScope _scope;

        public RepoCacheTestLoop(ILogger log, IRepositoryCache cache, ILifetimeScope scope)
        {
            _log = log;
            _cache = cache;
            _scope = scope;

            Console.CancelKeyPress += ConsoleCancelKeyPress;
        }

        private void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;

            Console.WriteLine();
            Console.WriteLine("Please use '#q' to quit the loop.");
            Console.Write(">");
        }

        public async Task RunAsync(CancellationToken token)
        {
            Console.WriteLine("This will connect to the PoshGit2 daemon process");
            Console.WriteLine();
            Console.WriteLine("Available commands:");
            Console.WriteLine("\t[path]     Search for a repo");
            Console.WriteLine("\t#q         Quit the loop");
            Console.WriteLine("\t#remove    Remove a repo from the cache");
            Console.WriteLine("\t#all       Retrieve all repos");
            Console.WriteLine();

            while (!token.IsCancellationRequested)
            {
                const string remove = "#remove ";

                Console.Write("> ");
                var line = Console.ReadLine();

                // This occurs if CTRL-C is pushed
                if(line == null)
                {
                    continue;
                }

                if (line == "#q")
                {
                    return;
                }
                else if (line == "#all")
                {
                    await ProcessGetAllCommandsAsync(token);
                }
                else if(line == "#clear")
                {
                    await ProcessClearCacheCommandAsync(token);
                }
                else if (line.StartsWith(remove, StringComparison.Ordinal))
                {
                    var path = line.Substring(remove.Length).Trim();

                    await ProcessRemoveRepoCommandAsync(path, token);
                }
                else
                {
                    var cwd = new StringCurrentWorkingDirectory(line);

                    await ProcessGetRepoCommandAsync(cwd, token);
                }
            }
        }

        private async Task<bool> ProcessRemoveRepoCommandAsync(string path, CancellationToken token)
        {
            var result = await _cache.RemoveRepoAsync(path, token);

            if (result)
            {
                _log.Information("Removed repo: {Path}", path);
            }
            else
            {
                _log.Warning("Failed to remove repo: {Path}", path);
            }

            return true;
        }

        private Task<bool> ProcessClearCacheCommandAsync(CancellationToken token)
        {
            return _cache.ClearCacheAsync(token);
        }

        private async Task<bool> ProcessGetAllCommandsAsync(CancellationToken token)
        {
            var result = await _cache.GetAllReposAsync(token);

            _log.Information("Found repos: {@Repos}", result);

            return true;
        }

        private async Task<bool> ProcessGetRepoCommandAsync(ICurrentWorkingDirectory cwd, CancellationToken token)
        {
            using (var scope = _scope.BeginLifetimeScope(builder => builder.RegisterInstance(cwd).As<ICurrentWorkingDirectory>()))
            {
                var repo = await scope.Resolve<Task<IRepositoryStatus>>();

                if (repo != null)
                {
                    _log.Information("{@Repo}", repo);
                }
                else
                {
                    _log.Information("No repo found: {@Input}", cwd);
                }
            }

            return true;
        }

        public void Dispose()
        {
            Console.CancelKeyPress -= ConsoleCancelKeyPress;
        }
    }
}
