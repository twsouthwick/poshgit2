using System;
using System.Threading;

namespace PoshGit2
{
    public class MutexThrottle : IThrottle
    {
        private readonly Mutex _mutex = new Mutex();
        private bool _isWaiting;

        public bool TryContinueOrBlock(Action action)
        {
            if (_mutex.WaitOne(0))
            {
                try
                {
                    action();
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

                    action();

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
    }
}