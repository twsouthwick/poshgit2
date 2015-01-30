using System.Management.Automation;

namespace PoshGit2.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "GitStatus")]
    public class GetGitInfo : DICmdlet
    {
        public IRepositoryCache RepositoryCache { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var repo = RepositoryCache.GetCurrentRepo();

            if (repo != null)
            {
                WriteObject(repo);
            }
        }
    }
}
