using System;
using System.Threading;

namespace PoshGit2
{
    public sealed class MutexThrottle : IThrottle, IDisposable
    {
        private readonly Mutex _mutex = new Mutex();
        private bool _isWaiting;

        public bool TryContinueOrBlock(Action action)
        {
            if (_mutex.WaitOne(0))
            {
                try
                {
                    action?.Invoke();
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }

                return true;
            }

            if (!_isWaiting)
            {
                try
                {
                    _isWaiting = true;
                    _mutex.WaitOne();

                    action?.Invoke();

                    return true;
                }
                finally
                {
                    _isWaiting = false;
                    _mutex.ReleaseMutex();
                }
            }

            return false;
        }

        public void Dispose()
        {
            _mutex.Dispose();
        }
    }
}