using Autofac;
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
        public string Format { get; set; }

        public ILifetimeScope Scope { get; set; }

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
            if (Format == null)
            {
                var repositoryStatus = await Scope.Resolve<Task<IRepositoryStatus>>();
                var writer = Scope.Resolve<IStatusWriter>();

                if (repositoryStatus != null)
                {
                    writer.WriteStatus(repositoryStatus);

                    var vt100 = writer as VT100StatusWriter;

                    return vt100?.Status;
                }

                return null;
            }
            else
            {
                var cache = Scope.Resolve<IRepositoryCache>();
                var cwd = Scope.Resolve<ICurrentWorkingDirectory>();

                var statusString = await cache.GetStatusStringAsync(Format, cwd, CancellationToken.None);

                return statusString;
            }
        }
    }
}
