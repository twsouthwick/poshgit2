using System.Management.Automation;

namespace PoshGit2.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "GitStatus")]
    public class GetGitInfo : AutofacCmdlet
    {
        public IRepositoryCache RepositoryCache { get; set; }
        public ICurrentWorkingDirectory WorkingDirectory { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var repo = RepositoryCache.FindRepo(WorkingDirectory);

            if (repo != null)
            {
                WriteObject(repo);
            }
        }
    }
}
