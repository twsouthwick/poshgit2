using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
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

        public async Task<IRepositoryStatus> FindRepo(ICurrentWorkingDirectory cwd, CancellationToken cancellationToken)
        {
            using (var pipe = new NamedPipeClientStream(NamedPipeRepoServer.ServerName, NamedPipeRepoServer.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                pipe.Connect();

                using (var writer = new NonClosingStreamWriter(pipe) { AutoFlush = true })
                using (var reader = new NonClosingStreamReader(pipe))
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
                }
            }
        }

        public Task<IEnumerable<IRepositoryStatus>> GetAllRepos(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRepo(IRepositoryStatus repo, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}