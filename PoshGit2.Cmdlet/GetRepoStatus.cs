using System;
using System.Management.Automation;
using System.Threading;

namespace PoshGit2
{
    [Cmdlet(VerbsCommon.Get, "GitRepo")]
    public class GetRepoStatus : AutofacCmdlet
    {
        public IRepositoryCache RepositoryCache { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            try
            {
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));

                foreach (var repo in RepositoryCache.GetAllRepos(cancellationTokenSource.Token).Result)
                {
                    WriteObject(repo);
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
