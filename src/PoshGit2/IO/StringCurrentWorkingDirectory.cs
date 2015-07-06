using System.IO;

namespace PoshGit2.IO
{
    public class StringCurrentWorkingDirectory : WindowsCurrentWorkingDirectory
    {
        public StringCurrentWorkingDirectory(string cwd)
        {
            CWD = cwd;
        }

        public override string CWD { get; }

        public override bool IsValid => Directory.Exists(CWD) || File.Exists(CWD);
    }
}
