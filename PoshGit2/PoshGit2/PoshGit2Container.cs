using Autofac;
using PoshGit2.Status;
using PoshGit2.Utils;
using System;
using System.Management.Automation;

namespace PoshGit2
{
    internal sealed class PoshGit2Container : IDisposable
    {
        private readonly static Lazy<PoshGit2Container> _instance = new Lazy<PoshGit2Container>(true);

        public static IContainer Instance { get { return _instance.Value.Container; } }

        public PoshGit2Container()
        {
            Container = CreateContainer();

            // Instantiate logger
            Container.Resolve<ILogger>();
        }

        public IContainer Container { get; }

        private IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<RepositoryCache>().As<IRepositoryCache>().SingleInstance();
            builder.RegisterType<PSCurrentWorkingDirectory>().As<ICurrentWorkingDirectory>().InstancePerLifetimeScope();
            builder.RegisterType<UpdateableRepositoryStatus>().As<IRepositoryStatus>();
            builder.RegisterType<LibGit2Sharp.Repository>().As<LibGit2Sharp.IRepository>();
            builder.RegisterType<FileWatcher>().As<IFileWatcher>();
            builder.RegisterType<FolderWatcher>().As<IFolderWatcher>();
            builder.RegisterType<MutexThrottle>().As<IThrottle>();
            builder.RegisterType<SessionState>().AsSelf().SingleInstance();
            builder.RegisterType<ConsoleStatusWriter>().As<IStatusWriter>().InstancePerLifetimeScope();
            builder.RegisterType<FileLogger>().As<ILogger>().SingleInstance();

            builder.Register(c =>
            {
                var cache = c.Resolve<IRepositoryCache>();
                var cwd = c.Resolve<ICurrentWorkingDirectory>();

                return new Option<IRepositoryStatus>(cache.FindRepo(cwd));
            }).As<Option<IRepositoryStatus>>().InstancePerLifetimeScope();

            builder.RegisterAdapter<SessionState, IGitPromptSettings>(s =>
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
                return new DefaultGitPromptSettings();
            });

            return builder.Build();
        }

        public void Dispose()
        {
            if (_instance.IsValueCreated)
            {
                _instance.Value.Dispose();
            }
        }
    }
}
