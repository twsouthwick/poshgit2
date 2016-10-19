using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    [Cmdlet(VerbsCommunications.Write, "GitStatus", DefaultParameterSetName = nameof(Console))]
    public class WriteGitStatus : AutofacCmdlet
    {
        [Parameter(Mandatory = false, ParameterSetName = nameof(VT100))]
        public SwitchParameter VT100 { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = nameof(PlainText))]
        public SwitchParameter PlainText { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = nameof(Console))]
        public SwitchParameter Console { get; set; } = true;

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
            else if (PlainText)
            {
                return await Cache.GetStatusStringAsync(null, Cwd, CancellationToken.None);
            }
            else if (Console)
            {
                var repositoryStatus = await Status.Value;

                if (repositoryStatus != null)
                {
                    Writer.WriteStatus(repositoryStatus);

                    return Writer.Status;
                }
            }

            return null;
        }
    }
}
