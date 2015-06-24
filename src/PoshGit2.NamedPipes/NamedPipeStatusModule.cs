using Autofac;
using System;
using System.Threading;

namespace PoshGit2
{
    public class NamedPipeStatusModule : Module
    {
        private bool _showServer;

        public bool ShowServer
        {
            get { return _showServer; }
            set { _showServer = value; }
        }

        public NamedPipeStatusModule()
        {
            bool.TryParse(Environment.GetEnvironmentVariable("poshgit2_showServer"), out _showServer);
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NamedPipePoshGitClient>()
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.Register(ctx =>
               {
                   var client = ctx.Resolve<NamedPipePoshGitClient>();

                   return new ServerStartupPoshGitClient(
                       client,
                       client,
                       ctx.Resolve<ILogger>(),
                       ShowServer);
               })
              .OnActivated(a => a.Instance.EnsureServerIsAvailable())
              .As<IRepositoryCache>()
              .As<ITabCompleter>()
              .InstancePerLifetimeScope();
        }
    }
}
