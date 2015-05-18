using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace PoshGit2
{
    internal enum NamedPipeCommand : byte
    {
        FindRepo,
        GetAllRepos,
        RemoveRepo,
        ClearCache,
        Ready,
        BadCommand,
        Success,
        Failed
    }

    internal static class ReaderWriterExtensions
    {
        internal static Task WriteAsync(this StreamWriter writer, NamedPipeCommand command)
        {
            return writer.WriteAsync((char)command);
        }

        internal static async Task<NamedPipeCommand> ReadCommandAsync(this StreamReader reader)
        {
            var buffer = new char[1];

            var result = await reader.ReadAsync(buffer, 0, 1);

            Debug.Assert(result == 1);

            return (NamedPipeCommand)buffer[0];
        }
    }
}
