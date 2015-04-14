using Autofac;
using PoshGit2;
using System.Threading;

namespace PoshGit.Daemon
{
    class Program
    {
        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new PoshGitAutofacModule { LogToConsole = true });
            builder.RegisterModule(new PoshGitDaemonModule());

            return builder.Build();
        }

        static void Main(string[] args)
        {
            using (var container = BuildContainer())
            {
                var loop = container.Resolve<RepoSearchLoop>();

                loop.RunAsync(CancellationToken.None).Wait();
            }
        }
    }
}
