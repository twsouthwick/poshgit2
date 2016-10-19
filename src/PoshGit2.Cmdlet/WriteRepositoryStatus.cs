using System.Management.Automation;
using System.Threading.Tasks;

namespace PoshGit2
{
    [Cmdlet(VerbsCommunications.Write, "RepositoryStatus")]
    public class WriteRepositoryStatus : AutofacCmdlet
    {
        public Task<IRepositoryStatus> Status { get; set; }

        public ConsoleStatusWriter Writer { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            ProcessRecordAsync().GetAwaiter().GetResult();
        }

        private async Task ProcessRecordAsync()
        {
            var repositoryStatus = await Status;

            if (repositoryStatus != null)
            {
                Writer.WriteStatus(repositoryStatus);
            }
        }
    }
}
