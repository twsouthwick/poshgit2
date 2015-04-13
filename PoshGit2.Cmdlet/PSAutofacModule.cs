using Autofac;
using PoshGit2.Status;
using System;
using System.Management.Automation;
using Serilog;
using System.IO;

namespace PoshGit2
{
    public class PSAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ExpiringCache>().As<IRepositoryCache>().SingleInstance();
            builder.RegisterType<PSCurrentWorkingDirectory>().As<ICurrentWorkingDirectory>().InstancePerLifetimeScope();
            builder.RegisterType<UpdateableRepositoryStatus>().As<IRepositoryStatus>();
            builder.RegisterType<LibGit2Sharp.Repository>().As<LibGit2Sharp.IRepository>();
            builder.RegisterType<SemaphoreThrottle>().As<IThrottle>();
            builder.RegisterType<GitFolderWatcher>().As<GitFolderWatcher>();
            builder.RegisterType<SampledFolderWatcher>().As<IFolderWatcher>()
                .WithParameter((p, c) => p.ParameterType == typeof(Func<string, IFolderWatcher>), (p, c) => c.Resolve<Func<string, GitFolderWatcher>>());
            builder.RegisterType<SessionState>().AsSelf().SingleInstance();
            builder.RegisterType<SessionStateWrapper>().As<ISessionState>().InstancePerLifetimeScope();
            builder.RegisterType<ConsoleStatusWriter>().As<IStatusWriter>().InstancePerLifetimeScope();
            builder.RegisterType<SerilogWrapper>().As<ILogger>().InstancePerLifetimeScope();
            builder.RegisterType<DefaultGitPromptSettings>().AsSelf().SingleInstance();
            builder.Register(CreateLogger).Named<Serilog.ILogger>("Logger")
                .SingleInstance();
            builder.RegisterDecorator<Serilog.ILogger>((c, l) => l.ForContext("scope", new { Type = "global" }), "Logger")
                .InstancePerLifetimeScope();

            builder.Register(c =>
            {
                var cache = c.Resolve<IRepositoryCache>();
                var cwd = c.Resolve<ICurrentWorkingDirectory>();

                return new Option<IRepositoryStatus>(cache.FindRepo(cwd));
            }).As<Option<IRepositoryStatus>>().InstancePerLifetimeScope();

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

        private Serilog.ILogger CreateLogger(IComponentContext arg)
        {
            var config = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .WriteTo.Trace()
                .WriteTo.RollingFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PoshGit2", "log-{Date}.txt"));

            var seqServer = Environment.GetEnvironmentVariable("poshgit2_seq_server");

            if (string.IsNullOrWhiteSpace(seqServer))
            {
                return config.CreateLogger();
            }
            else
            {
                var logger = config
                                   .WriteTo.Seq(seqServer)
                                   .CreateLogger();

                logger.Information("Seq Server {Address}", seqServer);

                return logger;
            }
        }
    }
}
