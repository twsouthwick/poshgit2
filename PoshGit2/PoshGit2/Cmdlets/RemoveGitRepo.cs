using System.Management.Automation;

namespace PoshGit2.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "GitRepo")]
    public class RemoveGitRepo : DICmdlet
    {
        [Parameter(ParameterSetName = "name", Mandatory = true)]
        public string RepositoryPath { get; set; }

        [Parameter(ParameterSetName = "repo", Mandatory = true)]
        public IRepositoryStatus Repository { get; set; }

        public IRepositoryCache RepositoryCache { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (Repository != null)
            {
                RepositoryCache.Remove(RepositoryPath ?? Repository.GitDir);
            }

            foreach (var repo in RepositoryCache.All)
            {
                WriteObject(repo);
            }
        }
    }
}
