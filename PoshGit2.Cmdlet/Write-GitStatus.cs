using PoshGit2.Status;
using System.Management.Automation;

namespace PoshGit2
{
    [Cmdlet(VerbsCommunications.Write, "GitStatus")]
    public class Write_GitStatus : AutofacCmdlet
    {
        public IStatusWriter Writer { get; set; }
        public Option<IRepositoryStatus> Status { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (Status.HasValue)
            {
                Writer.WriteStatus(Status.Value);
            }
        }
    }
}
