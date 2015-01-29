using System;

namespace PoshGit2
{
    public interface IFileWatcher : IObservable<FileChangedStatus>
    {
        void Pause();
        void Start();
    }
}