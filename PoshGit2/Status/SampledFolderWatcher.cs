using System;
using System.Reactive.Linq;

namespace PoshGit2
{
    public sealed class SampledFolderWatcher : IFolderWatcher, IDisposable
    {
        private readonly IFolderWatcher _innerFolderWatcher;
        private readonly IObservable<string> _observable;

        public SampledFolderWatcher(string folder, Func<string, IFolderWatcher> factory)
        {
            _innerFolderWatcher = factory(folder);
            _observable = _innerFolderWatcher.GetFileObservable().Sample(TimeSpan.FromMilliseconds(200));
        }

        public IObservable<string> GetFileObservable()
        {
            return _observable;
        }

        public void Dispose()
        {
            (_innerFolderWatcher as IDisposable)?.Dispose();
        }
    }
}