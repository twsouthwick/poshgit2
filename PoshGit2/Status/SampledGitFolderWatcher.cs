using System;
using System.Reactive.Linq;

namespace PoshGit2
{
    public sealed class SampledGitFolderWatcher : IFolderWatcher, IDisposable
    {
        private readonly GitFolderWatcher _gitFolderWatcher;
        private readonly IObservable<string> _observable;

        public SampledGitFolderWatcher(string folder)
        {
            _gitFolderWatcher = new GitFolderWatcher(folder);
            _observable = _gitFolderWatcher.GetFileObservable().Sample(TimeSpan.FromMilliseconds(200));
        }

        public IObservable<string> GetFileObservable()
        {
            return _observable;
        }

        public void Dispose()
        {
            _gitFolderWatcher.Dispose();
        }
    }
}