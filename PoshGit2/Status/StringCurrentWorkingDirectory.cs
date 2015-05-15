using System.IO;

namespace PoshGit2
{
    public class StringCurrentWorkingDirectory : WindowsCurrentWorkingDirectory
    {
        private readonly string _cwd;

        public StringCurrentWorkingDirectory(string cwd)
        {
            _cwd = cwd;
        }

        public override string CWD { get { return _cwd; } }

        public override bool IsValid
        {
            get
            {
                return Directory.Exists(_cwd) || File.Exists(_cwd);
            }
        }
    }
}
