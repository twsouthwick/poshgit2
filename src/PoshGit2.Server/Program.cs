using Autofac;
using PoshGit2;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PoshGit.Daemon
{
    class Program
    {
        static void Main(string[] args)
        {
            var argSet = new HashSet<string>(args, StringComparer.OrdinalIgnoreCase);

            var skipServer = argSet.Contains("test");
            var showServer = argSet.Contains("showServer");

            if (skipServer)
            {
                Console.Title = "PoshGit2 Server (Test)";

                RunTestLoop(showServer);
            }
            else
            {
                Console.Title = "PoshGit2 Server";

                TryStartServer();
            }
        }

        private static int GetPID()
        {
            using (var process = System.Diagnostics.Process.GetCurrentProcess())
            {
                return process.Id;
            }
        }

        private static bool TryStartServer()
        {
            bool createdNewServerMutex;
            using (var serverMutex = new Mutex(true, ServerInfo.Name, out createdNewServerMutex))
            {
                if (!createdNewServerMutex)
                {
                    return false;
                }

                using (var container = BuildServerContainer())
                {
                    var server = container.Resolve<NamedPipePoshGitServer>();

                    server.RunAsync(CancellationToken.None).Wait();
                }

                return true;
            }
        }

        private static void RunTestLoop(bool showServer)
        {
            using (var container = BuildTestLoopContainer(showServer))
            {
                var loop = container.Resolve<RepoCacheTestLoop>();

                loop.RunAsync(CancellationToken.None).Wait();
            }
        }

        private static IContainer BuildTestLoopContainer(bool showServer)
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new PoshGitAutofacModule());
            builder.RegisterModule(new SerilogModule { LogToConsole = true });
            builder.RegisterModule(new NamedPipeStatusModule { ShowServer = showServer });

            builder.RegisterType<TabCompleter>()
                .As<ITabCompleter>();

            builder.RegisterType<RepoCacheTestLoop>()
               .AsSelf()
               .SingleInstance();

            return builder.Build();
        }

        private static IContainer BuildServerContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new PoshGitAutofacModule());
            builder.RegisterModule(new SerilogModule
            {
                LogUnhandledExceptions = true,
                LogToConsole = true
            });

            builder.RegisterType<NamedPipePoshGitServer>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<TabCompleter>()
                .As<ITabCompleter>();

            return builder.Build();
        }
    }
}
