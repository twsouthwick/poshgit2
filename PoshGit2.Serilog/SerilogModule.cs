using Autofac;
using Autofac.Core;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace PoshGit2
{
    public class SerilogModule : Module
    {
        public bool LogToConsole { get; set; }

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
            var processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            var config = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithMachineName()
                .Destructure.ByTransforming<ReadonlyCopyRepositoryStatus>(ConvertStatus)
                .Destructure.ByTransforming<ReadWriteRepositoryStatus>(ConvertStatus)
                .WriteTo.Trace()
                .WriteTo.RollingFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PoshGit2", $"log-{processId}-{{Date}}.txt"));

            if (LogToConsole)
            {
                config = config.WriteTo.ColoredConsole();
            }

            var seqServer = Environment.GetEnvironmentVariable("poshgit2_seq_server");

            if (string.IsNullOrWhiteSpace(seqServer))
            {
                return config.CreateLogger();
            }
            else
            {
                var logger = config
                                   .WriteTo.Seq(seqServer)
                                   .CreateLogger();

                logger.Information("Seq Server {Address}", seqServer);

                return logger;
            }
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
