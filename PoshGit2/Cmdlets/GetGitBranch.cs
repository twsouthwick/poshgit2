using System.Management.Automation;

namespace PoshGit2.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "GitBranch")]
    public class GetGitBranch : AutofacCmdlet
    {
        [Parameter]
        public SwitchParameter Remote { get; set; }

        public Option<IRepositoryStatus> Status { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!Status.HasValue)
            {
                return;
            }

            foreach (var branch in Status.Value.LocalBranches)
            {
                WriteObject(branch);
            }

            if (Remote.IsPresent)
            {
                foreach (var branch in Status.Value.RemoteBranches)
                {
                    WriteObject(branch);
                }
            }
        }
    }
}
