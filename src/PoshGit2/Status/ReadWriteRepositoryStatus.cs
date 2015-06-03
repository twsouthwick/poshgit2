using LibGit2Sharp;
using System.Collections.Generic;

namespace PoshGit2
{
    public class ReadWriteRepositoryStatus : IRepositoryStatus
    {
        public int AheadBy { get; set; }

        public int BehindBy { get; set; }

        public string Branch { get; set; }

        public IReadOnlyCollection<ConfigurationEntry<string>> Configuration { get; set; }

        public string CurrentWorkingDirectory { get; set; }

        public string GitDir { get; set; }

        public ChangedItemsCollection Index { get; set; }

        public IReadOnlyCollection<string> LocalBranches { get; set; }

        public IReadOnlyCollection<string> RemoteBranches { get; set; }

        public IReadOnlyCollection<string> Remotes { get; set; }

        public IReadOnlyCollection<string> Stashes { get; set; }

        public ChangedItemsCollection Working { get; set; }
    }
}
