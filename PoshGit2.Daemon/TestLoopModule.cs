using Autofac;

namespace PoshGit2
{
    internal class TestLoopModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RepoSearchLoop>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<CommandLineInputLoop>()
                .As<IRepoSearch>()
                .SingleInstance();
        }
    }
}
