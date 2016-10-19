using Autofac;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public class PoshGitAutofacModule : Module
    {
        public TimeSpan TimeOut { get; set; } = TimeSpan.FromMinutes(2);

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ExpiringCache>()
                .As<IRepositoryCache>()
                .SingleInstance();

            builder.RegisterType<UpdateableRepositoryStatus>()
                .As<IRepositoryStatus>();

            builder.RegisterType<LibGit2Sharp.Repository>()
                .As<LibGit2Sharp.IRepository>();

            builder.RegisterType<GitFolderWatcher>()
                .As<IFolderWatcher>();

            builder.RegisterType<ConsoleStatusWriter>()
                .As<IStatusWriter>()
                .InstancePerLifetimeScope();

            builder.RegisterType<Settings.DefaultGitPromptSettings>()
                .AsSelf()
                .InstancePerDependency();

            builder.Register(_ => new CancellationTokenSource(TimeOut))
                .AsSelf()
                .InstancePerDependency();

            builder.RegisterAdapter<CancellationTokenSource, CancellationToken>(s => s.Token);

            builder.Register(c =>
                {
                    var cache = c.Resolve<IRepositoryCache>();
                    var cwd = c.Resolve<ICurrentWorkingDirectory>();
                    var cancellationToken = c.Resolve<CancellationToken>();

                    return cache.FindRepoAsync(cwd, cancellationToken);
                })
                .As<Task<IRepositoryStatus>>()
                .InstancePerLifetimeScope();

            builder.RegisterType<DefaultStatusWriterProvider>()
                .As<IStatusWriterProvider>()
                .SingleInstance();
        }
    }
}
