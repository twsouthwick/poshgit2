using Autofac;
using Autofac.Core;
using System;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace PoshGit2
{
    public class AutofacCmdlet : PSCmdlet, IDisposable
    {
        private static Lazy<IContainer> Container = new Lazy<IContainer>(CreateContainer, true);

        private readonly ILifetimeScope _lifetimeScope;

        private static IContainer CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterModule(new SerilogModule());
            containerBuilder.RegisterModule(new PoshGitAutofacModule());
            containerBuilder.RegisterModule(new PSAutofacModule());
            containerBuilder.RegisterModule(new NamedPipeStatusModule());

            return containerBuilder.Build();
        }

        static AutofacCmdlet()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledExceptionHandler;
        }

        private static void CurrentDomainUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var log = Container.Value.Resolve<ILogger>();

            if (e.IsTerminating)
            {
                log.Fatal(e.ExceptionObject as Exception, "Terminating exception: {Sender}", sender);
            }
            else
            {
                log.Warning(e.ExceptionObject as Exception, "Non-terminating exception: {Sender}", sender);
            }
        }

        public AutofacCmdlet()
        {
            _lifetimeScope = Container.Value.BeginLifetimeScope(builder =>
           {
               builder.Register<SessionState>(_ => SessionState).AsSelf();
               builder.RegisterDecorator<Serilog.ILogger>((c, l) => l.ForContext("scope", new { Type = "cmdlet", Cmdlet = this.GetType().Name }, true), fromKey: "Logger")
                      .As<Serilog.ILogger>()
                      .InstancePerLifetimeScope();
           });
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            _lifetimeScope.InjectProperties(this, CmdletParameterSelector.Instance);
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _lifetimeScope.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private sealed class CmdletParameterSelector : IPropertySelector
        {
            public static IPropertySelector Instance = new CmdletParameterSelector();

            public bool InjectProperty(PropertyInfo propertyInfo, object instance)
            {
                return !propertyInfo.GetCustomAttributes<ParameterAttribute>().Any();
            }
        }
    }
}
