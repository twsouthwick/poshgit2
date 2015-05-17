using Autofac;
using Autofac.Core;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PoshGit2
{
    public class SerilogModule : Module
    {
        public SerilogModule()
        {
            var processId = Process.GetCurrentProcess().Id;

            LogToConsole = false;
            LogToTrace = true;
            SeqServer = Environment.GetEnvironmentVariable("poshgit2_seq_server");
            LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PoshGit2", $"log-{processId}-{{Date}}.txt");
        }

        public bool LogToConsole { get; set; }

        public string SeqServer { get; set; }

        public string LogFile { get; set; }

        public bool LogToTrace { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SerilogWrapper>()
                .As<ILogger>()
                .InstancePerLifetimeScope();

            builder.Register(CreateLogger)
                .Named<Serilog.ILogger>("Logger")
                .SingleInstance();

            builder.RegisterDecorator<Serilog.ILogger>((c, l) => l.ForContext("scope", new { Type = "global" }), "Logger")
                .InstancePerLifetimeScope();
        }

        private static void AddILoggerToParameters(object sender, PreparingEventArgs e)
        {
            var t = e.Component.Activator.LimitType;
            var resolvedParameter = new ResolvedParameter((p, i) => p.ParameterType == typeof(ILogger), (p, i) => new SerilogWrapper(i.Resolve<Serilog.ILogger>().ForContext(t)));

            e.Parameters = e.Parameters.Union(new[] { resolvedParameter });
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            registration.Preparing += AddILoggerToParameters;
        }

        private Serilog.ILogger CreateLogger(IComponentContext arg)
        {
            var config = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithMachineName()
                .Destructure.ByTransforming<ReadonlyCopyRepositoryStatus>(ConvertStatus)
                .Destructure.ByTransforming<ReadWriteRepositoryStatus>(ConvertStatus)
                .Destructure.ByTransforming<ProcessStartInfo>(p => new { Name = p.FileName, Args = p.Arguments, WindowStyle = p.WindowStyle, WorkingDirectory = p.WorkingDirectory });

            if (LogToTrace)
            {
                config = config.WriteTo.Trace();
            }

            if (!string.IsNullOrWhiteSpace(LogFile))
            {
                config = config.WriteTo.RollingFile(LogFile);
            }

            if (LogToConsole)
            {
                config = config.WriteTo.ColoredConsole();
            }

            if (!string.IsNullOrWhiteSpace(SeqServer))
            {
                config = config.WriteTo.Seq(SeqServer);
            }

            return config.CreateLogger();
        }

        private object ConvertStatus(IRepositoryStatus status)
        {
            if (status == null)
            {
                return new { };
            }

            return new
            {
                GitDir = status.GitDir,
                Index = status.Index?.ToString(),
                Working = status.Working?.ToString(),
                Branch = status.Branch
            };
        }
    }
}
