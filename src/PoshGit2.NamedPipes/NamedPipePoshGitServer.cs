using Autofac;
using Newtonsoft.Json;
using PoshGit2.IO;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public sealed class NamedPipePoshGitServer : IDisposable
    {
        public static readonly string ServerName = ".";

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _log;
        private readonly IRepositoryCache _repoCache;
        private readonly JsonSerializer _serializer;
        private readonly ILifetimeScope _lifetimeScope;

        public NamedPipePoshGitServer(IRepositoryCache repoCache, ILifetimeScope lifetimeScope, ILogger log)
        {
            _log = log;
            _cancellationTokenSource = new CancellationTokenSource();
            _repoCache = repoCache;
            _serializer = JsonSerializer.Create();
            _lifetimeScope = lifetimeScope;

            log.Information("Server started");
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await PrivateRunAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    _log.Error(e, "Unexpected error in NamedPipeRepoServer");
                }
            }
        }

        private async Task PrivateRunAsync(CancellationToken cancellationToken)
        {
            using (var pipe = new NamedPipeServerStream(ServerInfo.Name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await pipe.WaitForConnectionAsync(cancellationToken);

                    using (var reader = new NonClosingStreamReader(pipe))
                    using (var writer = new NonClosingStreamWriter(pipe) { AutoFlush = true })
                    {
                        var input = await reader.ReadCommandAsync();

                        _log.Information("Retrieved input {Input}", input);

                        switch (input)
                        {
                            case NamedPipeCommand.FindRepo:
                                await writer.WriteAsync(NamedPipeCommand.Ready);
                                await FindRepo(writer, await reader.ReadLineAsync(), cancellationToken);
                                break;
                            case NamedPipeCommand.GetAllRepos:
                                await writer.WriteAsync(NamedPipeCommand.Ready);
                                await GetAllRepos(writer, cancellationToken);
                                break;
                            case NamedPipeCommand.RemoveRepo:
                                await writer.WriteAsync(NamedPipeCommand.Ready);
                                await RemoveRepo(writer, reader, cancellationToken);
                                break;
                            case NamedPipeCommand.ClearCache:
                                await writer.WriteAsync(NamedPipeCommand.Ready);
                                await ProcessClearCacheAsync(writer, cancellationToken);
                                break;
                            case NamedPipeCommand.ExpandGitCommand:
                                await writer.WriteAsync(NamedPipeCommand.Ready);
                                await ProcessExpandGitCommandAsync(writer, reader, cancellationToken);
                                break;
                            default:
                                await writer.WriteAsync(NamedPipeCommand.BadCommand);
                                break;
                        }
                    }

                    // This must be after the reader and writer are closed
                    // Otherwise, an InvalidOperationException is thrown
                    pipe.WaitForPipeDrain();
                    pipe.Disconnect();
                }
            }
        }

        private async Task ProcessExpandGitCommandAsync(StreamWriter writer, StreamReader reader, CancellationToken cancellationToken)
        {
            var cwd = await reader.ReadLineAsync();
            var line = await reader.ReadLineAsync();
            var scwd = new StringCurrentWorkingDirectory(cwd);

            using (var scope = _lifetimeScope.BeginLifetimeScope(b => b.RegisterInstance(scwd).As<ICurrentWorkingDirectory>()))
            {
                var tabCompleter = scope.Resolve<ITabCompleter>();

                var result = await tabCompleter.CompleteAsync(line, cancellationToken);

                using (var jsonTextWriter = new JsonTextWriter(writer))
                {
                    _serializer.Serialize(jsonTextWriter, result);
                }
            }

            _log.Information("Expanded git command '{Line}'", line);
        }

        private async Task ProcessClearCacheAsync(StreamWriter writer, CancellationToken cancellationToken)
        {
            if (await _repoCache.ClearCacheAsync(cancellationToken))
            {
                await writer.WriteAsync(NamedPipeCommand.Success);

                _log.Information("Cleared cache");
            }
            else
            {
                await writer.WriteAsync(NamedPipeCommand.Failed);

                _log.Warning("Failed to clear cache");
            }
        }

        private async Task RemoveRepo(StreamWriter writer, StreamReader reader, CancellationToken cancellationToken)
        {
            var repoPath = await reader.ReadLineAsync();
            var result = await _repoCache.RemoveRepoAsync(repoPath, cancellationToken);

            if (result)
            {
                await writer.WriteAsync(NamedPipeCommand.Success);

                _log.Information("Removed repo {Repo}", repoPath);
            }
            else
            {
                await writer.WriteAsync(NamedPipeCommand.Failed);

                _log.Warning("Failed to remove repo {Repo}", repoPath);
            }
        }

        private async Task GetAllRepos(StreamWriter writer, CancellationToken cancellationToken)
        {
            var all = await _repoCache.GetAllReposAsync(cancellationToken);

            using (var jsonTextWriter = new JsonTextWriter(writer))
            {
                _serializer.Serialize(jsonTextWriter, all);
            }

            _log.Information("Retrieved all repos");
        }

        private async Task FindRepo(StreamWriter writer, string path, CancellationToken cancellationToken)
        {
            var cwd = new StringCurrentWorkingDirectory(path);

            var repo = await _repoCache.FindRepoAsync(cwd, cancellationToken);

            if (repo == null)
            {
                _log.Warning("Did not find a repo at '{Path}'", path);

                await writer.WriteLineAsync(string.Empty);
            }
            else
            {
                using (var jsonTextWriter = new JsonTextWriter(writer))
                {
                    _serializer.Serialize(jsonTextWriter, repo);
                }

                _log.Information("Found a repo at '{Path}'", path);
            }
        }
    }
}
