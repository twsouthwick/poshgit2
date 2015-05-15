using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public sealed class NamedPipeRepoServer : IDisposable
    {
        public const string PipeName = "PoshGit2";
        public static readonly string ServerName = Environment.MachineName;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _log;
        private readonly IRepositoryCache _repoCache;
        private readonly JsonSerializer _serializer;

        public NamedPipeRepoServer(IRepositoryCache repoCache, ILogger log)
        {
            _log = log;
            _cancellationTokenSource = new CancellationTokenSource();
            _repoCache = repoCache;
            _serializer = JsonSerializer.Create();

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
            using (var pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Factory.FromAsync(pipe.BeginWaitForConnection, pipe.EndWaitForConnection, null);

                    _log.Information("Connection started");

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

        private async Task RemoveRepo(StreamWriter writer, StreamReader reader, CancellationToken cancellationToken)
        {
            var repoPath = await reader.ReadLineAsync();
            var result = await _repoCache.RemoveRepoAsync(repoPath, cancellationToken);

            await writer.WriteAsync(result ? NamedPipeCommand.Success : NamedPipeCommand.Failed);
        }

        private async Task GetAllRepos(StreamWriter writer, CancellationToken cancellationToken)
        {
            var all = await _repoCache.GetAllReposAsync(cancellationToken);

            using (var jsonTextWriter = new JsonTextWriter(writer))
            {
                _serializer.Serialize(jsonTextWriter, all);
            }
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

            using (var jsonTextWriter = new JsonTextWriter(writer))
            {
                _serializer.Serialize(jsonTextWriter, repo);
            }
        }
    }
}
