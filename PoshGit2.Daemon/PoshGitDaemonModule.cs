using Autofac;

namespace PoshGit2
{
    internal class PoshGitDaemonModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RepoSearchLoop>().AsSelf();
            builder.RegisterType<CommandLineInputLoop>().As<IRepoSearch>();
            //builder.RegisterType<StringCurrentWorkingDirectory>().As<ICurrentWorkingDirectory>();
        }
    }
}
