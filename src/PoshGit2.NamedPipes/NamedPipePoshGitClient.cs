using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public class NamedPipePoshGitClient : IRepositoryCache, ITabCompleter
    {
        private readonly ILogger _log;
        private readonly JsonSerializer _serializer;
        private readonly ICurrentWorkingDirectory _cwd;

        public NamedPipePoshGitClient(ILogger log, ICurrentWorkingDirectory cwd)
        {
            _log = log;
            _cwd = cwd;
            _serializer = Serializer.Instance;
        }

        public Task<IRepositoryStatus> FindRepoAsync(ICurrentWorkingDirectory cwd, CancellationToken cancellationToken)
        {
            return SendReceiveCommandAsync(async (reader, writer) =>
            {
                await writer.WriteLineAsync(cwd.CWD);

                using (var jsonReader = new JsonTextReader(reader))
                {
                    return _serializer
                        .Deserialize<ReadWriteRepositoryStatus>(jsonReader) as IRepositoryStatus;
                }
            }, NamedPipeCommand.FindRepo, cancellationToken);
        }

        public Task<string> GetStatusStringAsync(IGitPromptSettings settings, ICurrentWorkingDirectory cwd, CancellationToken cancellationToken)
        {
            return SendReceiveCommandAsync(async (reader, writer) =>
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    _serializer.Serialize(jsonWriter, new StatusStringData
                    {
                        Cwd = cwd.CWD,
                        Settings = settings
                    });
                }

                return await reader.ReadToEndAsync();
            }, NamedPipeCommand.StatusString, cancellationToken);
        }

        public Task<IEnumerable<IRepositoryStatus>> GetAllReposAsync(CancellationToken cancellationToken)
        {
            return SendReceiveCommandAsync((reader, writer) =>
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    var result = _serializer
                        .Deserialize<IEnumerable<ReadWriteRepositoryStatus>>(jsonReader)
                        .Cast<IRepositoryStatus>();

                    return Task.FromResult(result);
                }
            }, NamedPipeCommand.GetAllRepos, cancellationToken, Enumerable.Empty<IRepositoryStatus>());
        }

        public Task<bool> RemoveRepoAsync(string path, CancellationToken cancellationToken)
        {
            return SendReceiveCommandAsync(async (reader, writer) =>
            {
                await writer.WriteLineAsync(path);

                var result = await reader.ReadCommandAsync();

                return result == NamedPipeCommand.Success;
            }, NamedPipeCommand.RemoveRepo, cancellationToken);
        }

        public Task<bool> ClearCacheAsync(CancellationToken cancellationToken)
        {
            return SendReceiveCommandAsync(async (reader, writer) =>
            {
                var result = await reader.ReadCommandAsync();

                return result == NamedPipeCommand.Success;
            }, NamedPipeCommand.ClearCache, cancellationToken);
        }

        public Task<TabCompletionResult> CompleteAsync(string line, CancellationToken cancellationToken)
        {
            return SendReceiveCommandAsync(async (reader, writer) =>
            {
                await writer.WriteLineAsync(_cwd.CWD);
                await writer.WriteLineAsync(line);

                using (var jsonReader = new JsonTextReader(reader))
                {
                    return _serializer
                        .Deserialize<TabCompletionResult>(jsonReader);
                }
            }, NamedPipeCommand.ExpandGitCommand, cancellationToken, TabCompletionResult.Failure);
        }

        private async Task<T> SendReceiveCommandAsync<T>(Func<StreamReader, StreamWriter, Task<T>> func, NamedPipeCommand command, CancellationToken cancellationToken, T defaultValue = default(T))
        {
#if DEBUG
            var timeout = TimeSpan.FromDays(1);
#else
            var timeout = TimeSpan.FromSeconds(2);
#endif

            // Time out after 2 seconds to access named pipe
            using (var innerCancellationTokenSource = new CancellationTokenSource(timeout))
            {
                try
                {
                    // Ensure that the named pipe cancellation token gets canceled if the main token is
                    using (cancellationToken.Register(innerCancellationTokenSource.Cancel))
                    using (var pipe = new NamedPipeClientStream(NamedPipePoshGitServer.ServerName, ServerInfo.Name, PipeDirection.InOut, PipeOptions.Asynchronous))
                    {
                        await pipe.ConnectAsync(innerCancellationTokenSource.Token);

                        using (var writer = new NonClosingStreamWriter(pipe) { AutoFlush = true })
                        using (var reader = new NonClosingStreamReader(pipe))
                        {
                            await writer.WriteAsync(command);

                            var response = await reader.ReadCommandAsync();

                            if (response != NamedPipeCommand.Ready)
                            {
                                return defaultValue;
                            }

                            return await func(reader, writer);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _log.Warning("Named pipe communication with server was canceled");

                    return defaultValue;
                }
                catch (IOException e)
                {
                    _log.Warning(e, "IOException in named pipe communication");

                    return defaultValue;
                }
                catch (Exception e)
                {
                    _log.Error(e, "{InnerCancellationToken} {CancellationToken}", innerCancellationTokenSource.IsCancellationRequested, cancellationToken.IsCancellationRequested);

                    return defaultValue;
                }
            }
        }
    }
}