using System;
using System.Management.Automation;
using System.Threading;

namespace PoshGit2
{
    [Cmdlet(VerbsCommon.Get, "GitStatus")]
    public class GetGitInfo : AutofacCmdlet
    {
        public IRepositoryCache RepositoryCache { get; set; }
        public ICurrentWorkingDirectory WorkingDirectory { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
                var repo = RepositoryCache.FindRepo(WorkingDirectory, cancellationTokenSource.Token).Result;

                if (repo != null)
                {
                    WriteObject(repo);
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
