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

        public CancellationToken Token { get; set; }

        public ILogger Log { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                var repo = RepositoryCache.FindRepoAsync(WorkingDirectory, Token).Result;

                if (repo != null)
                {
                    WriteObject(repo);
                }
            }
            catch (OperationCanceledException)
            {
                Log.Error("GetGitInfo timedout");
            }
        }
    }
}
