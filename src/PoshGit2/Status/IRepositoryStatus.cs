using LibGit2Sharp;
using System.Collections.Generic;

namespace PoshGit2
{
    public interface IRepositoryStatus
    {
        string Branch { get; }
        int AheadBy { get; }
        ChangedItemsCollection Working { get; }
        ChangedItemsCollection Index { get; }
        int BehindBy { get; }
        string GitDir { get; }
        string CurrentWorkingDirectory { get; }
        IReadOnlyCollection<string> LocalBranches { get; }
        IReadOnlyCollection<string> RemoteBranches { get; }
        IReadOnlyCollection<string> Stashes { get; }
        IReadOnlyCollection<string> Remotes { get; }
        IReadOnlyCollection<ConfigurationEntry<string>> Configuration { get; }
    }
}