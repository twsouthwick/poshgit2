using System;

namespace PoshGit2
{
    public interface IThrottle
    {
        bool TryContinueOrBlock(Action action);
    }
}