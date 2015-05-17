using System.Management.Automation;

namespace PoshGit2
{
    public class SessionStateWrapper : ISessionState
    {
        private readonly SessionState _sessionState;

        public SessionStateWrapper(SessionState sessionState)
        {
            _sessionState = sessionState;
        }

        public PSVariableIntrinsics PSVariable { get { return _sessionState.PSVariable; } }
        public PathIntrinsics Path { get { return _sessionState.Path; } }
    }
}