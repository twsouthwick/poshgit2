using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace PoshGit2
{
    public interface IServerInformation : IDisposable
    {
        Process Process { get; }
    }

    internal sealed class ServerSideServerInformation : IServerInformation
    {
        private MemoryMappedFile _file;

        public ServerSideServerInformation(ILogger log)
        {
            _file = MemoryMappedFile.CreateNew(ServerInfo.FileName, 8);

            using (var stream = _file.CreateViewStream())
            using (var writer = new BinaryWriter(stream))
            using (var p = Process)
            {
                writer.Write(p.Id);

                log.Information("Wrote {PID} to {MemoryMappedFile}", p.Id, ServerInfo.FileName);
            }
        }

        public Process Process => Process.GetCurrentProcess();

        public void Dispose()
        {
            _file.Dispose();
            _file = null;
        }
    }

    internal class ClientSideServerInformation : IServerInformation
    {
        private readonly ILogger _log;

        public ClientSideServerInformation(ILogger log)
        {
            _log = log;
        }

        public Process Process
        {
            get
            {
                try
                {
                    using (var file = MemoryMappedFile.OpenExisting(ServerInfo.FileName))
                    using (var stream = file.CreateViewStream())
                    using (var reader = new BinaryReader(stream))
                    {
                        if (stream.Length > 0)
                        {
                            var pid = reader.ReadInt32();

                            return Process.GetProcessById(pid);
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e, "Error access server PID with {ServerName}", ServerInfo.FileName);
                }

                return null;
            }
        }

        public void Dispose()
        {
        }
    }
}
