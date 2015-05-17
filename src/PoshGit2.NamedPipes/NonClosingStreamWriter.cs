using System.IO;
using System.Text;

namespace PoshGit2
{
    internal class NonClosingStreamWriter : StreamWriter
    {
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

        public NonClosingStreamWriter(Stream stream)
            : base(stream, DefaultEncoding, 1024, true)
        { }
    }
}
