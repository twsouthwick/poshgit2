using System.Management.Automation;

namespace PoshGit2.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "GitStashList")]
    public class GetGitStashList : AutofacCmdlet
    {
        public Option<IRepositoryStatus> Status { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!Status.HasValue)
            {
                return;
            }
            
            foreach(var stash in Status.Value.Stashes)
            {
                WriteObject(stash);
            }
        }
    }
}
