using LibGit2Sharp;
using System.Collections.Generic;

namespace PoshGit2
{
    public class ReadWriteRepositoryStatus : IRepositoryStatus
    {
        public int AheadBy { get; set; }

        public int BehindBy { get; set; }

        public string Branch { get; set; }

        public IEnumerable<ConfigurationEntry<string>> Configuration { get; set; }

        public string CurrentWorkingDirectory { get; set; }

        public string GitDir { get; set; }

        public ChangedItemsCollection Index { get; set; }

        public IEnumerable<string> LocalBranches { get; set; }

        public IEnumerable<string> RemoteBranches { get; set; }

        public IEnumerable<string> Remotes { get; set; }

        public IEnumerable<string> Stashes { get; set; }

        public ChangedItemsCollection Working { get; set; }
    }
}
