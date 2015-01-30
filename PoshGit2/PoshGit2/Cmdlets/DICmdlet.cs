using Autofac;
using System.Management.Automation;

namespace PoshGit2.Cmdlets
{
    public class DICmdlet : Cmdlet
    {
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            PoshGit2Container.Instance.InjectUnsetProperties(this);
        }
    }
}
