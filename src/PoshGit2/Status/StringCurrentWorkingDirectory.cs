using System.IO;

namespace PoshGit2
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
