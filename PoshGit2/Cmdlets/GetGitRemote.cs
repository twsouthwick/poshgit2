using System.Management.Automation;

namespace PoshGit2.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "GitRemote")]
    public class GetGitRemote : AutofacCmdlet
    {
        public Option<IRepositoryStatus> Status { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!Status.HasValue)
            {
                return;
            }

            foreach (var remote in Status.Value.Remotes)
            {
                WriteObject(remote);
            }
        }
    }
}
