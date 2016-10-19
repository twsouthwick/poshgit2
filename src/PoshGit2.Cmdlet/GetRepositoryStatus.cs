using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    [Cmdlet(VerbsCommon.Get, "RepositoryStatus", DefaultParameterSetName = nameof(PlainText))]
    public class GetRepositoryStatus : AutofacCmdlet
    {
        [Parameter(Mandatory = false, ParameterSetName = nameof(VT100))]
        public SwitchParameter VT100 { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = nameof(PlainText))]
        public SwitchParameter PlainText { get; set; }

        public IRepositoryCache Cache { get; set; }

        public ICurrentWorkingDirectory Cwd { get; set; }

        public IGitPromptSettings Settings { get; set; }

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
            if (ParameterSetName == nameof(VT100))
            {
                if (!VirtualTerminalHelper.IsEnabled)
                {
                    throw new InvalidOperationException("VT100 is not available on your system");
                }

                return await Cache.GetStatusStringAsync(Settings, Cwd, CancellationToken.None);
            }
            else if (ParameterSetName == nameof(PlainText))
            {
                return await Cache.GetStatusStringAsync(null, Cwd, CancellationToken.None);
            }

            return null;
        }
    }
}
