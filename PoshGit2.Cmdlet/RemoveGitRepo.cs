using System.Management.Automation;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace PoshGit2
{
    [Cmdlet(VerbsCommon.Remove, "GitRepo")]
    public class RemoveGitRepo : AutofacCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public IRepositoryStatus[] Repository { get; set; }

        public IRepositoryCache RepositoryCache { get; set; }

        public CancellationToken CancellationToken { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // TODO: Add overload for IEnumerable
            var removing = Repository.Select(repo => RepositoryCache.RemoveRepo(repo, CancellationToken)).ToArray();

            Task.WaitAll(removing);

            foreach (var repo in RepositoryCache.GetAllRepos(CancellationToken).Result)
            {
                WriteObject(repo);
            }
        }
    }
}
