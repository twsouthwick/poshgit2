using System.Management.Automation;

namespace PoshGit2
{
    public interface ISessionState
    {
        PathIntrinsics Path { get; }
        PSVariableIntrinsics PSVariable { get; }
    }
}