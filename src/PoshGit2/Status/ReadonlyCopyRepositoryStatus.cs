using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PoshGit2
{
    public class ReadonlyCopyRepositoryStatus : IRepositoryStatus
    {
        public ReadonlyCopyRepositoryStatus(IRepositoryStatus status, ICurrentWorkingDirectory cwd = null)
        {
            Branch = status.Branch;
            AheadBy = status.AheadBy;
            BehindBy = status.BehindBy;
            GitDir = status.GitDir;
            LocalBranches = status.LocalBranches;
            RemoteBranches = status.RemoteBranches;
            Stashes = status.Stashes;
            Remotes = status.Remotes;
            Configuration = status.Configuration;

            if (cwd == null)
            {
                CurrentWorkingDirectory = status.CurrentWorkingDirectory;
                Working = status.Working;
                Index = status.Index;
            }
            else
            {
                CurrentWorkingDirectory = cwd.CWD;
                Working = UpdateItemPaths(status.Working, cwd);
                Index = UpdateItemPaths(status.Index, cwd);
            }
        }

        public ChangedItemsCollection UpdateItemPaths(ChangedItemsCollection items, ICurrentWorkingDirectory cwd)
        {
            return new ChangedItemsCollection
            {
                Added = UpdatePaths(items.Added, cwd),
                Deleted = UpdatePaths(items.Deleted, cwd),
                Modified = UpdatePaths(items.Modified, cwd),
                Unmerged = UpdatePaths(items.Unmerged, cwd),
            };
        }

        private IReadOnlyCollection<string> UpdatePaths(IEnumerable<string> paths, ICurrentWorkingDirectory cwd)
        {
            return paths.Select(p => cwd.CreateRelativePath(Path.GetFullPath(Path.Combine(GitDir, "..", p))))
                .ToList()
                .AsReadOnly();
        }

        public string Branch { get; }
        public int AheadBy { get; }
        public ChangedItemsCollection Working { get; }
        public ChangedItemsCollection Index { get; }
        public int BehindBy { get; }
        public string GitDir { get; }
        public string CurrentWorkingDirectory { get; }
        public IReadOnlyCollection<string> LocalBranches { get; }
        public IReadOnlyCollection<string> RemoteBranches { get; }
        public IReadOnlyCollection<string> Stashes { get; }
        public IReadOnlyCollection<string> Remotes { get; }
        public IReadOnlyCollection<ConfigurationEntry<string>> Configuration { get; }
    }
}
