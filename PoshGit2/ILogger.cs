using System;

namespace PoshGit2
{
    public interface ILogger
    {
        void LogException(Exception e, bool isTerminating);
    }
}