using Autofac;
using System;
using System.IO;
using System.Text;

namespace PoshGit2
{
    internal sealed class PoshGit2Container : IDisposable
    {
        private static Lazy<PoshGit2Container> _instance = new Lazy<PoshGit2Container>(true);
        public static IContainer Instance { get { return _instance.Value.Container; } }

        public PoshGit2Container()
        {
            Container = CreateContainer();
        }

        public IContainer Container { get; }

        private IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<RepoFinder>().As<IRepositoryCache>().SingleInstance();
            builder.RegisterType<PSCurrentWorkingDirectory>().As<ICurrentWorkingDirectory>().SingleInstance();
            builder.RegisterType<UpdateableRepositoryStatus>().As<IRepositoryStatus>();
            builder.RegisterType<LibGit2Sharp.Repository>().As<LibGit2Sharp.IRepository>();
            builder.RegisterType<FileWatcher>().As<IFileWatcher>();
            builder.RegisterType<FolderWatcher>().As<IFolderWatcher>();

            AppDomain.CurrentDomain.FirstChanceException += FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            return builder.Build();
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ExceptionWriter(e.ExceptionObject as Exception, e.IsTerminating);
        }

        private static void FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            ExceptionWriter(e.Exception, false);
        }

        private static void ExceptionWriter(Exception e, bool isTerminating)
        {
            var sb = new StringBuilder();
            var terminating = isTerminating ? "Terminating" : "Continuing";

            sb.AppendLine("-----------------------------------");
            sb.AppendLine($"Date: {DateTime.Now} [{System.Diagnostics.Process.GetCurrentProcess().Id}] | {terminating}");
            sb.AppendLine($"{e}");
            sb.AppendLine("-----------------------------------");

            var path = $@"{Path.GetTempPath()}\poshgit2-{Guid.NewGuid()}.err.log";

            File.AppendAllText(path, sb.ToString());
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.FirstChanceException -= FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException -= UnhandledException;

            Container.Dispose();
        }
    }
}
