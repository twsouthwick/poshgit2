using System;

namespace PoshGit2
{
    public interface IThrottle
    {
        void TryContinueOrBlock(Action action);
    }
}