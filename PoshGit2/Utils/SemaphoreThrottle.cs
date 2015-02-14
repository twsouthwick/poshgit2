using System;
using System.Threading;

namespace PoshGit2.Utils
{
    public class SemaphoreThrottle : IThrottle
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
                action();

                return true;
            }
            finally
            {
                Monitor.Exit(_lock);
                _semaphore.Release();
            }
        }
    }
}
