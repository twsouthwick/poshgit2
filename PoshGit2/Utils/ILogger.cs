using System;

namespace PoshGit2.Utils
{
    public interface ILogger
    {
        void LogException(Exception e, bool isTerminating);
    }
}