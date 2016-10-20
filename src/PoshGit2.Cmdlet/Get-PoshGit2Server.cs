using System.Management.Automation;

namespace PoshGit2
{
    [Cmdlet(VerbsCommon.Get, "PoshGit2Server")]
    public class GetPoshGit2Server : AutofacCmdlet
    {
        public IServerInformation ServerInformation { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var process = ServerInformation.Process;

            if (process != null)
            {
                WriteObject(process);
            }
        }
    }
}
