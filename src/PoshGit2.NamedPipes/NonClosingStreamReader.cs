using System.IO;
using System.Text;

namespace PoshGit2
{
    internal class NonClosingStreamReader : StreamReader
    {
        public NonClosingStreamReader(Stream stream)
            : base(stream, Encoding.UTF8, true, 1024, true)
        { }
    }
}
