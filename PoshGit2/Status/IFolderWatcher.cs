using System;

namespace PoshGit2
{
    public interface IFolderWatcher 
    {
        IObservable<string> GetFileObservable();
    }
}