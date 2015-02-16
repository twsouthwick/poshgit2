using System;

namespace PoshGit2
{
    public sealed class SampledFolderWatcher : IFolderWatcher, IDisposable
    {
        private readonly IFolderWatcher _innerFolderWatcher;
        private readonly IThrottle _throttle;

        public SampledFolderWatcher(string folder, IThrottle throttle, Func<string, IFolderWatcher> factory)
        {
            _innerFolderWatcher = factory(folder);
            _throttle = throttle;

            _innerFolderWatcher.OnNext += ThrottleOnNext;
        }

        private void ThrottleOnNext(string obj)
        {
            _throttle.TryContinueOrBlock(() => OnNext?.Invoke(obj));
        }

        public event Action<string> OnNext;

        public void Dispose()
        {
            _innerFolderWatcher.OnNext -= ThrottleOnNext;
            (_innerFolderWatcher as IDisposable)?.Dispose();
        }
    }
}