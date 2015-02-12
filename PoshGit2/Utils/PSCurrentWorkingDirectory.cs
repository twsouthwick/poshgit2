using Microsoft.PowerShell.Commands;
using System.Management.Automation;

namespace PoshGit2
{
    public class Foo 
    {
        public int SomeValue { get; set; }
    }

    public class Bar
    {
        public int SomeValue()
        {
            var foo = new Foo();

            return foo.SomeValue;
        }
    }
    public class PSCurrentWorkingDirectory : ICurrentWorkingDirectory
    {
        private readonly SessionState _sessionState;
        private readonly ICurrentWorkingDirectory _otherCwd;

        public PSCurrentWorkingDirectory(SessionState sessionState, ICurrentWorkingDirectory otherCwd)
        {
            _sessionState = sessionState;
            _otherCwd = otherCwd;
        }

        public bool IsValid
        {
            get
            {
                return _sessionState.Path.CurrentLocation.Provider.ImplementingType == typeof(FileSystemProvider);
            }
        }

        public string CWD
        {
            get
            {
                return _sessionState.Path.CurrentLocation.ProviderPath;
            }
        }

        public string CreateRelativePath(string path)
        {
            return _otherCwd.CreateRelativePath(path);
        }
    }
}