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
            // TODO: Clone ChangeItemsCollection entries
            HasWorking = status.HasWorking;
            Branch = status.Branch;
            AheadBy = status.AheadBy;
            HasUntracked = status.HasUntracked;
            HasIndex = status.HasIndex;
            BehindBy = status.BehindBy;
            GitDir = status.GitDir;
            LocalBranches = status.LocalBranches.ToList();
            RemoteBranches = status.RemoteBranches.ToList();
            Stashes = status.Stashes.ToList();
            Remotes = status.Remotes.ToList();
            Configuration = status.Configuration.ToList();

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

        private ICollection<string> UpdatePaths(IEnumerable<string> paths, ICurrentWorkingDirectory cwd)
        {
            return paths.Select(p => cwd.CreateRelativePath(Path.GetFullPath(Path.Combine(GitDir, "..", p)))).ToList();
        }

        public bool HasWorking { get; }
        public string Branch { get; }
        public int AheadBy { get; }
        public ChangedItemsCollection Working { get; }
        public bool HasUntracked { get; }
        public ChangedItemsCollection Index { get; }
        public bool HasIndex { get; }
        public int BehindBy { get; }
        public string GitDir { get; }
        public string CurrentWorkingDirectory { get; }
        public IEnumerable<string> LocalBranches { get; }
        public IEnumerable<string> RemoteBranches { get; }
        public IEnumerable<string> Stashes { get; }
        public IEnumerable<string> Remotes { get; }
        public IEnumerable<ConfigurationEntry<string>> Configuration { get; }
    }
}
