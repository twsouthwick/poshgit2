using PoshGit2.Status;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PoshGit2
{
    [Cmdlet(VerbsCommunications.Write, "GitStatus")]
    public class Write_GitStatus : AutofacCmdlet
    {
        public IStatusWriter Writer { get; set; }

        public Task<IRepositoryStatus> Status { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var status = Status.Result;

            if (status != null)
            {
                Writer.WriteStatus(status);
            }
        }
    }
}
