using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;

namespace PoshGit2
{
    public class FileLogger : ILogger
    {
        private readonly string _path;

        public FileLogger()
        {
            _path = SetupLogging();
        }

        public void LogException(Exception e, bool isTerminating)
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
            LogException(e.ExceptionObject as Exception, e.IsTerminating);
        }

        private void FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            LogException(e.Exception, false);
        }
    }
}
