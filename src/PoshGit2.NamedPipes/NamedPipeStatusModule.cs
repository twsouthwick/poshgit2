using Autofac;
using System;

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

        public bool ServerMode { get; set; }

        public NamedPipeStatusModule()
        {
            bool.TryParse(Environment.GetEnvironmentVariable("poshgit2_showServer"), out _showServer);
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (ServerMode)
            {
                builder.RegisterType<NamedPipePoshGitServer>()
                    .AsSelf()
                    .SingleInstance();

                builder.RegisterType<ServerSideServerInformation>()
                    .As<IServerInformation>()
                    .SingleInstance();
            }
            else
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

                builder.RegisterType<ClientSideServerInformation>()
                    .As<IServerInformation>()
                    .SingleInstance();
            }
        }
    }
}
