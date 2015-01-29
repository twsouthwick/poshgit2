using Autofac;
using System.Management.Automation;

namespace PoshGit2
{
    [Cmdlet(VerbsCommon.Get, "GitStatus")]
    public class GetGitInfo : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            PoshGit2Container.Instance.InjectUnsetProperties(this);
        }

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
