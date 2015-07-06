using Microsoft.PowerShell.Commands;

namespace PoshGit2.IO
{
    public class PSCurrentWorkingDirectory : WindowsCurrentWorkingDirectory
    {
        private readonly ISessionState _sessionState;

        public PSCurrentWorkingDirectory(ISessionState sessionState)
        {
            _sessionState = sessionState;
        }

        public override bool IsValid
        {
            get
            {
                return _sessionState.Path.CurrentLocation.Provider.ImplementingType == typeof(FileSystemProvider);
            }
        }

        public override string CWD
        {
            get
            {
                return _sessionState.Path.CurrentLocation.ProviderPath;
            }
        }
    }
}