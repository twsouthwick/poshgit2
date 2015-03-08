using System.Collections.Generic;

namespace PoshGit2
{
    public interface IRepositoryStatus
    {
        bool HasWorking { get; }
        string Branch { get; }
        int AheadBy { get; }
        ChangedItemsCollection Working { get; }
        bool HasUntracked { get; }
        ChangedItemsCollection Index { get; }
        bool HasIndex { get; }
        int BehindBy { get; }
        string GitDir { get; }
        string CurrentWorkingDirectory { get; }
        IEnumerable<string> LocalBranches { get; }
        IEnumerable<string> RemoteBranches { get; }
        IEnumerable<string> Stashes { get; }
        IEnumerable<string> Remotes { get; }
    }
}