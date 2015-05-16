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
        private readonly JsonSerializer _serializer;

        public NamedPipeRepoCache()
        {
            _serializer = JsonSerializer.Create();
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
            }, NamedPipeCommand.FindRepo);
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
            }, NamedPipeCommand.GetAllRepos);
        }

        public Task<bool> RemoveRepoAsync(string path, CancellationToken cancellationToken)
        {
            return SendReceiveCommandAsync(async (reader, writer) =>
            {
                await writer.WriteLineAsync(path);

                var result = await reader.ReadCommandAsync();

                return result == NamedPipeCommand.Success;
            }, NamedPipeCommand.RemoveRepo);
        }

        private async Task<T> SendReceiveCommandAsync<T>(Func<StreamReader, StreamWriter, Task<T>> func, NamedPipeCommand command)
        {
            using (var pipe = new NamedPipeClientStream(NamedPipeRepoServer.ServerName, NamedPipeRepoServer.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                pipe.Connect();

                using (var writer = new NonClosingStreamWriter(pipe) { AutoFlush = true })
                using (var reader = new NonClosingStreamReader(pipe))
                {
                    await writer.WriteAsync(command);

                    var response = await reader.ReadCommandAsync();

                    if (response != NamedPipeCommand.Ready)
                    {
                        return default(T);
                    }

                    return await func(reader, writer);
                }
            }
        }
    }
}