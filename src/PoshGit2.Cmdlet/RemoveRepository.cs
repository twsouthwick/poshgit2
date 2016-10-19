using System.Management.Automation;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace PoshGit2
{
    [Cmdlet(VerbsCommon.Remove, "Repository", DefaultParameterSetName = nameof(Repository))]
    public class RemoveRepository : AutofacCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = nameof(Repository))]
        public string[] Repository { get; set; }

        [Parameter(ParameterSetName = nameof(All))]
        public SwitchParameter All { get; set; }

        public IRepositoryCache RepositoryCache { get; set; }

        public CancellationToken CancellationToken { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Remove().GetAwaiter().GetResult();
        }

        private async Task Remove()
        {
            if (All)
            {
                await RepositoryCache.ClearCacheAsync(CancellationToken);
            }
            else
            {
                var removing = Repository.Select(repo => RepositoryCache.RemoveRepoAsync(repo, CancellationToken));

                await Task.WhenAll(removing);
            }

            foreach (var repo in RepositoryCache.GetAllReposAsync(CancellationToken).Result)
            {
                WriteObject(repo);
            }
        }
    }
}
