using Autofac;
using System.Management.Automation;

namespace PoshGit2
{
    public class PSAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PSCurrentWorkingDirectory>().As<ICurrentWorkingDirectory>().InstancePerLifetimeScope();
            builder.RegisterType<SessionState>().AsSelf().SingleInstance();
            builder.RegisterType<SessionStateWrapper>().As<ISessionState>().InstancePerLifetimeScope();

            builder.RegisterAdapter<ISessionState, IGitPromptSettings>((c, s) =>
            {
                // If available, use from session information
                if (s.PSVariable != null)
                {
                    var psobject = s.PSVariable.GetValue("GitPromptSettings") as PSObject;

                    if (psobject != null)
                    {
                        return new PSObjectGitPromptSettings(psobject);
                    }
                }

                // Otherwise, use default settings
                return c.Resolve<DefaultGitPromptSettings>();
            });
        }
    }
}
