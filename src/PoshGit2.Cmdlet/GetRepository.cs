using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    [Cmdlet(VerbsCommon.Get, "Repository", DefaultParameterSetName = nameof(CurrentDirectory))]
    public class GetRepository : AutofacCmdlet
    {
        [Parameter(ParameterSetName = nameof(CurrentDirectory))]
        public SwitchParameter CurrentDirectory { get; set; }

        [Parameter(ParameterSetName = nameof(All))]
        public SwitchParameter All { get; set; }

        public IRepositoryCache RepositoryCache { get; set; }

        public ICurrentWorkingDirectory WorkingDirectory { get; set; }

        public CancellationToken Token { get; set; }

        public ILogger Log { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var repos = ProcessAsync().GetAwaiter().GetResult();

            foreach (var repo in repos)
            {
                WriteObject(repo);
            }
        }

        private async Task<IEnumerable<IRepositoryStatus>> ProcessAsync()
        {
            if (ParameterSetName == nameof(All))
            {
                return await RepositoryCache.GetAllReposAsync(Token);
            }
            else if (ParameterSetName == nameof(CurrentDirectory))
            {
                var repo = await RepositoryCache.FindRepoAsync(WorkingDirectory, Token);

                if (repo != null)
                {
                    return new[] { repo };
                }
            }

            return Enumerable.Empty<IRepositoryStatus>();
        }
    }
}
