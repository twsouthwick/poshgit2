using Autofac;
using System;
using System.Management.Automation;

namespace PoshGit2.Cmdlets
{
    public class DICmdlet : PSCmdlet, IDisposable
    {
        private readonly ILifetimeScope _lifetimeScope;

        public DICmdlet()
        {
            _lifetimeScope = PoshGit2Container.Instance.BeginLifetimeScope(builder =>
           {
               builder.Register<SessionState>(_ => SessionState).AsSelf();
           });
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            // TODO: This needs to be here for now, otherwise SessionState is not defined yet.
            // TODO: Is this called on each time something is piped through?
            _lifetimeScope.InjectUnsetProperties(this);
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
    }
}
