using System;

namespace PoshGit2
{
    public interface IFolderWatcher 
    {
        event Action<string> OnNext;
    }
}