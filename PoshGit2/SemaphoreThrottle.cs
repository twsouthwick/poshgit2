using System;
using System.Threading;

namespace PoshGit2
{
    public sealed class SemaphoreThrottle : IThrottle, IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(2);
        private readonly object _lock = new object();

        public bool TryContinueOrBlock(Action action)
        {
            if (!_semaphore.Wait(0))
            {
                return false;
            }

            try
            {
                Monitor.Enter(_lock);
                action?.Invoke();

                return true;
            }
            finally
            {
                Monitor.Exit(_lock);
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}
