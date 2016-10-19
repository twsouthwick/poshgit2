using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    [Cmdlet(VerbsCommunications.Write, "GitStatus")]
    public class WriteGitStatus : AutofacCmdlet
    {
        [Parameter(Mandatory = false, Position = 0)]
        public SwitchParameter VT100 { get; set; }

        public IRepositoryCache Cache { get; set; }

        public ICurrentWorkingDirectory Cwd { get; set; }

        public IGitPromptSettings Settings { get; set; }

        public Lazy<Task<IRepositoryStatus>> Status { get; set; }

        public IStatusWriter Writer { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var result = ProcessRecordAsync().GetAwaiter().GetResult();

            if (result != null)
            {
                WriteObject(result);
            }
        }

        private async Task<string> ProcessRecordAsync()
        {
            if (VT100)
            {
                if (!VirtualTerminalHelper.IsEnabled)
                {
                    throw new InvalidOperationException("VT100 is not available on your system");
                }

                return await Cache.GetStatusStringAsync(Settings, Cwd, CancellationToken.None);
            }
            else
            {
                var repositoryStatus = await Status.Value;

                Writer?.WriteStatus(repositoryStatus);

                return null;
            }
        }
    }
}
