using Autofac;
using PoshGit2;
using System;
using System.Threading;

namespace PoshGit.Daemon
{
    class Program
    {
        static void Main(string[] args)
        {
            if (TryStartServer())
            {
                return;
            }

#if DEBUG
            var test = true;
#else
            var test = args.Length > 0 && string.Equals("test", args[0], StringComparison.Ordinal);
#endif

            if (test)
            {
                RunTestLoop();
            }
        }

        private static bool TryStartServer()
        {
            bool createdNewServerMutex;
            using (var serverMutex = new Mutex(true, "PoshGit2_Server", out createdNewServerMutex))
            {
                if (!createdNewServerMutex)
                {
                    return false;
                }

                Console.WriteLine("Starting server...");

                using (var container = BuildServerContainer())
                {
                    var server = container.Resolve<NamedPipeRepoServer>();

                    server.RunAsync(CancellationToken.None).Wait();
                }

                return true;
            }
        }

        private static void RunTestLoop()
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                using (var container = BuildTestLoopContainer())
                {
                    var loop = container.Resolve<RepoSearchLoop>();

                    loop.RunAsync(CancellationToken.None).Wait();
                }
            }
        }

        private static IContainer BuildTestLoopContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new PoshGitAutofacModule { LogToConsole = true });
            builder.RegisterModule(new PoshGitDaemonModule());

            return builder.Build();
        }
        private static IContainer BuildServerContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new PoshGitAutofacModule { LogToConsole = true });
            builder.RegisterModule(new PoshGitDaemonModule());

            builder.RegisterType<ExpiringCache>().As<IRepositoryCache>().SingleInstance();
            builder.RegisterType<NamedPipeRepoServer>().AsSelf().SingleInstance();

            return builder.Build();
        }
    }
}
