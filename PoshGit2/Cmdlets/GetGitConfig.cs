using System.Management.Automation;

namespace PoshGit2.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "GitConfig")]
    public class GetGitConfig : AutofacCmdlet
    {
        public Option<IRepositoryStatus> Status { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!Status.HasValue)
            {
                return;
            }

            foreach (var config in Status.Value.Configuration)
            {
                WriteObject(config);
            }
        }
    }
}
