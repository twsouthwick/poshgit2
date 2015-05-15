﻿using Autofac;
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
                RunTestLoop(showServer);
            }
            else
            {
                TryStartServer();
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

                using (var container = BuildServerContainer())
                {
                    var server = container.Resolve<NamedPipeRepoServer>();

                    server.RunAsync(CancellationToken.None).Wait();
                }

                return true;
            }
        }

        private static void RunTestLoop(bool showServer)
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                using (var container = BuildTestLoopContainer(showServer))
                {
                    var loop = container.Resolve<RepoSearchLoop>();

                    loop.RunAsync(CancellationToken.None).Wait();
                }
            }
        }

        private static IContainer BuildTestLoopContainer(bool showServer)
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new PoshGitAutofacModule { LogToConsole = true });
            builder.RegisterModule(new PoshGitDaemonModule { ShowServer = showServer });

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
