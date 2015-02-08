using Microsoft.PowerShell.Commands;
using System.Management.Automation;

namespace PoshGit2
{
    class PSCurrentWorkingDirectory : ICurrentWorkingDirectory
    {
        private SessionState _sessionState;

        public PSCurrentWorkingDirectory(SessionState sessionState)
        {
            _sessionState = sessionState;
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
    }
}