using Autofac;
using System;
using System.Management.Automation;

namespace PoshGit2
{
    public class PSAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PSCurrentWorkingDirectory>()
                .As<ICurrentWorkingDirectory>()
                .InstancePerLifetimeScope();

            builder.RegisterType<SessionState>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<SessionStateWrapper>()
                .As<ISessionState>()
                .InstancePerLifetimeScope();

            // Retrieve the console background color
            builder.RegisterAdapter<ISessionState, ConsoleColor>((c, s) =>
            {
                const ConsoleColor DefaultBackground = ConsoleColor.DarkBlue;

                if (s.PSVariable == null)
                {
                    return DefaultBackground;
                }

                var host = s.PSVariable.GetValue("Host") as System.Management.Automation.Host.PSHost;

                if(host == null)
                {
                    return DefaultBackground;
                }

                return host.UI.RawUI.BackgroundColor;
            });

            builder.RegisterAdapter<ISessionState, IGitPromptSettings>((c, s) =>
            {
                var defaultSettings = c.Resolve<DefaultGitPromptSettings>();

                // If available, use from session information
                if (s.PSVariable != null)
                {
                    var psobject = s.PSVariable.GetValue("GitPromptSettings") as PSObject;

                    if (psobject != null)
                    {
                        return new PSObjectGitPromptSettings(psobject, defaultSettings);
                    }
                }

                // Otherwise, use default settings
                return defaultSettings;
            });
        }
    }
}
