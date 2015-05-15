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
            builder.Register(ctx =>
               {
                   var namedPipe = new NamedPipeRepoCache(ctx.Resolve<CancellationToken>());

                   return new DaemonStartupRepoCache(namedPipe, ctx.Resolve<ILogger>(), ShowServer);
               })
              .OnActivated(a => a.Instance.EnsureServerIsAvailable())
              .As<IRepositoryCache>()
              .SingleInstance();
        }
    }
}
