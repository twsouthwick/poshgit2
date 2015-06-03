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
        IEnumerable<string> LocalBranches { get; }
        IEnumerable<string> RemoteBranches { get; }
        IEnumerable<string> Stashes { get; }
        IEnumerable<string> Remotes { get; }
        IEnumerable<ConfigurationEntry<string>> Configuration { get; }
    }
}