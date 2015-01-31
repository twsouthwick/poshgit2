using Autofac;
using System;
using System.IO;
using System.Text;

namespace PoshGit2
{
    internal sealed class PoshGit2Container : IDisposable
    {
        private readonly static Lazy<PoshGit2Container> _instance = new Lazy<PoshGit2Container>(true);
        private readonly string _path;

        public static IContainer Instance { get { return _instance.Value.Container; } }

        public PoshGit2Container()
        {
            Container = CreateContainer();

            _path = SetupLogging();
        }

        public IContainer Container { get; }

        private IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<RepositoryCache>().As<IRepositoryCache>().SingleInstance();
            builder.RegisterType<PSCurrentWorkingDirectory>().As<ICurrentWorkingDirectory>().SingleInstance();
            builder.RegisterType<UpdateableRepositoryStatus>().As<IRepositoryStatus>();
            builder.RegisterType<LibGit2Sharp.Repository>().As<LibGit2Sharp.IRepository>();
            builder.RegisterType<FileWatcher>().As<IFileWatcher>();
            builder.RegisterType<FolderWatcher>().As<IFolderWatcher>();
            builder.RegisterType<MutexThrottle>().As<IThrottle>();

            return builder.Build();
        }

        private string SetupLogging()
        {
            var dir = Path.Combine(Path.GetTempPath(), "poshgit2");
            var path = Path.Combine(dir, $"{Guid.NewGuid()}.err.log");

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            AppDomain.CurrentDomain.FirstChanceException += FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            return path;
        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ExceptionWriter(e.ExceptionObject as Exception, e.IsTerminating);
        }

        private void FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            ExceptionWriter(e.Exception, false);
        }

        private void ExceptionWriter(Exception e, bool isTerminating)
        {
            try
            {
                var sb = new StringBuilder();
                var terminating = isTerminating ? "Terminating" : "Continuing";

                sb.AppendLine("-----------------------------------");
                sb.AppendLine($"Date: {DateTime.Now} [{System.Diagnostics.Process.GetCurrentProcess().Id}] | {terminating}");
                sb.AppendLine($"{e}");
                sb.AppendLine("-----------------------------------");

                File.AppendAllText(_path, sb.ToString());
            }
            catch { }
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.FirstChanceException -= FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException -= UnhandledException;

            Container.Dispose();
        }
    }
}
