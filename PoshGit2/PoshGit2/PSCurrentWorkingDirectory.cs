using System.Management.Automation;

namespace PoshGit2
{
    class PSCurrentWorkingDirectory : ICurrentWorkingDirectory
    {
        private SessionState _sessionState = new SessionState();

        public string CWD
        {
            get
            {
                return _sessionState.Path.CurrentFileSystemLocation.Path;
            }
        }
    }
}