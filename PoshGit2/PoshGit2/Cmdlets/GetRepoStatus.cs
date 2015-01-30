using System.Management.Automation;

namespace PoshGit2.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "GitRepo")]
    public class GetRepoStatus : DICmdlet
    {
        public IRepositoryCache RepositoryCache { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            foreach(var repo in RepositoryCache.All)
            {
                WriteObject(repo);
            }
        }
    }
}
