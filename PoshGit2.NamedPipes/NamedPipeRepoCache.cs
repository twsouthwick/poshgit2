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
    public class NamedPipeRepoCache : IRepositoryCache
    {
        private readonly CancellationToken _cancellationToken;
        private readonly JsonSerializer _serializer;

        public NamedPipeRepoCache(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _serializer = JsonSerializer.Create();
        }

        private async Task<T> SendReceiveCommandAsync<T>(Func<StreamReader, StreamWriter, Task<T>> func)
        {
            using (var pipe = new NamedPipeClientStream(NamedPipeRepoServer.ServerName, NamedPipeRepoServer.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                pipe.Connect();

                using (var writer = new NonClosingStreamWriter(pipe) { AutoFlush = true })
                using (var reader = new NonClosingStreamReader(pipe))
                {
                    return await func(reader, writer);
                }
            }
        }

        public Task<IRepositoryStatus> FindRepoAsync(ICurrentWorkingDirectory cwd, CancellationToken cancellationToken)
        {
            return SendReceiveCommandAsync(async (reader, writer) =>
            {
                await writer.WriteLineAsync(Commands.FindRepo);

                var response = await reader.ReadLineAsync();

                if (!string.Equals(Commands.Ready, response, StringComparison.Ordinal))
                {
                    return null;
                }

                await writer.WriteLineAsync(cwd.CWD);

                using (var jsonReader = new JsonTextReader(reader))
                {
                    return _serializer
                        .Deserialize<ReadWriteRepositoryStatus>(jsonReader) as IRepositoryStatus;
                }
            });
        }

        public Task<IEnumerable<IRepositoryStatus>> GetAllReposAsync(CancellationToken cancellationToken)
        {
            return SendReceiveCommandAsync(async (reader, writer) =>
            {
                await writer.WriteLineAsync(Commands.GetAllRepos);

                var response = await reader.ReadLineAsync();

                if (!string.Equals(Commands.Ready, response, StringComparison.Ordinal))
                {
                    return null;
                }

                using (var jsonReader = new JsonTextReader(reader))
                {
                    return _serializer
                        .Deserialize<IEnumerable<ReadWriteRepositoryStatus>>(jsonReader)
                        .Cast<IRepositoryStatus>();
                }
            });
        }

        public Task RemoveRepoAsync(string path, CancellationToken cancellationToken)
        {
            return SendReceiveCommandAsync(async (reader, writer) =>
            {
                await writer.WriteLineAsync(Commands.RemoveRepo);

                var response = await reader.ReadLineAsync();

                if (!string.Equals(Commands.Ready, response, StringComparison.Ordinal))
                {
                    return false;
                }

                await writer.WriteLineAsync(path);

                var result = await reader.ReadLineAsync();

                return string.Equals(Commands.Success, result, StringComparison.Ordinal);
            });
        }
    }
}