using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PoshGit2
{
    public sealed class UpdateableRepositoryStatus : IDisposable, IRepositoryStatus
    {
        private const string Elipses = "...";

        private readonly ICurrentWorkingDirectory _cwd;
        private readonly ILogger _log;

        // These are set in an initialization method so that it does not stall the console output
        private IRepository _repository;
        private IFolderWatcher _folderWatcher;

        private bool _isUpdating;

        public UpdateableRepositoryStatus(string folder, ILogger log, Func<string, IRepository> repositoryFactory, Func<string, IFolderWatcher> folderWatcherFactory, ICurrentWorkingDirectory cwd)
        {
            _log = log;
            _cwd = cwd;
            _isUpdating = true;

            Working = new ChangedItemsCollection();
            Index = new ChangedItemsCollection();

            Task.Run(() => Initialize(folder, repositoryFactory, folderWatcherFactory));
        }

        private void Initialize(string folder, Func<string, IRepository> repositoryFactory, Func<string, IFolderWatcher> folderWatcherFactory)
        {
            _repository = repositoryFactory(folder);

            _folderWatcher = folderWatcherFactory(folder);
            _folderWatcher.OnNext += UpdateStatus;

            // _repository.Info.Path returns a path ending with '\'
            GitDir = _repository.Info.Path.Substring(0, _repository.Info.Path.Length - 1);
            CurrentWorkingDirectory = Path.GetFullPath(Path.Combine(GitDir, ".."));

            UpdateStatus(CurrentWorkingDirectory);
        }

        public string Branch => _isUpdating ? $"{ExpandBranchName()}{Elipses}" : ExpandBranchName();

        private string ExpandBranchName()
        {
            // Repository may still be initializing
            if (_repository == null)
            {
                return Elipses;
            }

            var branch = _repository.Head.Name;

            if (_cwd.CWD.StartsWith(GitDir, StringComparison.CurrentCultureIgnoreCase))
            {
                return "GIT!";
            }

            if (Directory.Exists($@"{GitDir}\rebase-merge)"))
            {
                var rebase_branch = File.ReadAllText($@"{GitDir}\rebase-merge\head-name").TrimEnd().Replace("refs/heads/", "");

                if (File.Exists($@"{GitDir}\rebase-merge\interactive)"))
                {
                    return $"{rebase_branch}|REBASE-i";
                }

                return $"{rebase_branch}|REBASE-m";
            }

            if (Directory.Exists($@"{GitDir}\rebase-apply"))
            {
                if (File.Exists($@"{GitDir}\rebase-apply\rebasing"))
                {
                    return $"{branch}|REBASE";
                }
                else if (File.Exists($@"{GitDir}\rebase-apply\applying"))
                {
                    return $"{branch}|AM";
                }
                else
                {
                    return $"{branch}|AM/REBASE";
                }
            }

            if (File.Exists($@"{GitDir}\MERGE_HEAD"))
            {
                return $"{branch}|MERGING";
            }
            else
            {
                if (File.Exists($@"{GitDir}\CHERRY_PICK_HEAD"))
                {
                    return $"{branch}|CHERRY-PICKING";
                }

                if (File.Exists($@"{GitDir}\BISECT_LOG"))
                {
                    return $"{branch}|BISECTING";
                }
            }

            return branch;
        }

        public bool HasWorking { get { return Working.HasAny; } }
        public ChangedItemsCollection Working { get; set; }
        public bool HasUntracked { get; set; }
        public ChangedItemsCollection Index { get; set; }
        public bool HasIndex { get { return Index.HasAny; } }
        public string GitDir { get; set; }
        public string CurrentWorkingDirectory { get; set; }
        public int AheadBy { get { return _repository.Head.TrackingDetails.AheadBy ?? 0; } }
        public int BehindBy { get { return _repository.Head.TrackingDetails.BehindBy ?? 0; } }

        public IEnumerable<string> LocalBranches
        {
            get
            {
                return _repository.Branches.Where(b => !b.IsRemote).Select(b => b.Name);
            }
        }

        public IEnumerable<string> RemoteBranches
        {
            get
            {
                return _repository.Branches.Where(b => b.IsRemote).Select(b => b.Name);
            }
        }

        public IEnumerable<string> Stashes
        {
            get
            {
                return _repository.Stashes.Select(s => s.Name);
            }
        }

        public IEnumerable<string> Remotes
        {
            get
            {
                return _repository.Network.Remotes.Select(r => r.Name);
            }
        }

        public IEnumerable<ConfigurationEntry<string>> Configuration
        {
            get
            {
                return _repository.Config;
            }
        }

        public void UpdateStatus(string file)
        {
            _isUpdating = true;

            _log.Debug("Updating repo: {Path}", file);

            try
            {
                var repositoryStatus = _repository.RetrieveStatus();

                Working = new ChangedItemsCollection
                {
                    Added = GetCollection(repositoryStatus.Untracked),
                    Modified = GetCollection(repositoryStatus.Modified, repositoryStatus.RenamedInWorkDir),
                    Deleted = GetCollection(repositoryStatus.Missing)
                };

                Index = new ChangedItemsCollection
                {
                    Added = GetCollection(repositoryStatus.Added),
                    Modified = GetCollection(repositoryStatus.Staged, repositoryStatus.RenamedInIndex),
                    Deleted = GetCollection(repositoryStatus.Removed)
                };
            }
            catch (LibGit2SharpException e)
            {
                _log.Warning(e, "Unexpected git exception");
            }

            _log.Debug("Done updating repo {Path}", file);

            _isUpdating = false;
        }

        private ICollection<string> GetCollection(params IEnumerable<StatusEntry>[] entries)
        {
            return entries.SelectMany(o => o).Select(f => f.FilePath).ToList().AsReadOnly();
        }

        public void Dispose()
        {
            _repository.Dispose();

            _folderWatcher.OnNext -= UpdateStatus;
            (_folderWatcher as IDisposable)?.Dispose();
        }
    }
}
