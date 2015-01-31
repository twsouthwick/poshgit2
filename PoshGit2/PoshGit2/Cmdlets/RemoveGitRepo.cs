using System.Collections.Generic;
using System.Management.Automation;

namespace PoshGit2.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "GitRepo")]
    public class RemoveGitRepo : DICmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public IRepositoryStatus[] Repository { get; set; }

        public IRepositoryCache RepositoryCache { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            foreach (var repo in Repository)
            {
                RepositoryCache.Remove(repo);
            }

            foreach (var repo in RepositoryCache.All)
            {
                WriteObject(repo);
            }
        }
    }
}
