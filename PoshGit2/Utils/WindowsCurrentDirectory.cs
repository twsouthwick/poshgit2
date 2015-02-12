using System;

namespace PoshGit2.Utils
{
    public class WindowsCurrentDirectory : ICurrentWorkingDirectory
    {
        public string CWD { get { return Environment.CurrentDirectory; } }

        public bool IsValid { get { return true; } }
    }
}
