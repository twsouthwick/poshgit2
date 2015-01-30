using System;

namespace PoshGit2
{
    public interface IQueuedLocker
    {
        void TryContinueOrBlock(Action action);
    }
}