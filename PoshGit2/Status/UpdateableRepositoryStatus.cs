﻿using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PoshGit2
{
    public sealed class UpdateableRepositoryStatus : IDisposable, IRepositoryStatus
    {
        private readonly IRepository _repository;
        private readonly IFolderWatcher _folderWatcher;
        private readonly IThrottle _gate;
        private readonly ICurrentWorkingDirectory _cwd;

        private bool _isUpdating;

        public UpdateableRepositoryStatus(string folder, Func<string, IRepository> repositoryFactory, Func<string, IFolderWatcher> folderWatcherFactory, IThrottle gate, ICurrentWorkingDirectory cwd)
        {
            _gate = gate;
            _repository = repositoryFactory(folder);
            _cwd = cwd;

            _folderWatcher = folderWatcherFactory(folder);
            _folderWatcher.Subscribe(new StringDelegateObserver(_ => UpdateStatus()));

            // _repository.Info.Path returns a path ending with '\'
            GitDir = _repository.Info.Path.Substring(0, _repository.Info.Path.Length - 1);
            CurrentWorkingDirectory = Path.GetFullPath(Path.Combine(GitDir, ".."));

            Working = new ChangedItemsCollection();
            Index = new ChangedItemsCollection();

            UpdateStatus();
        }

        public string Branch
        {
            get
            {
                var name = GetExtraBranchStatus();

                return _isUpdating ? $"{name}..." : name;
            }
        }

        private string GetExtraBranchStatus()
        {
            var branch = _repository.Head.Name;

            if (_cwd.CWD.StartsWith(GitDir, StringComparison.CurrentCultureIgnoreCase))
            {
                return "GIT!";
            }

            if (Directory.Exists($"{GitDir}\rebase-merge)"))
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
        public string GitDir { get; }
        public string CurrentWorkingDirectory { get; }
        public int AheadBy { get { return _repository.Head.TrackingDetails.AheadBy ?? 0; } }
        public int BehindBy { get { return _repository.Head.TrackingDetails.BehindBy ?? 0; } }

        public void UpdateStatus()
        {
            _gate.TryContinueOrBlock(() =>
            {
                _isUpdating = true;

                Trace.WriteLine($"Updating repo {GitDir}");

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

                    Trace.WriteLine($"Done updating repo {GitDir}");
                }
                catch (LibGit2SharpException) { }

                _isUpdating = false;
            });
        }

        private ICollection<string> GetCollection(params IEnumerable<StatusEntry>[] entries)
        {
            return entries.SelectMany(o => o).Select(f => f.FilePath).ToList().AsReadOnly();
        }

        public void Dispose()
        {
            _repository.Dispose();
            (_folderWatcher as IDisposable)?.Dispose();
        }

        private class StringDelegateObserver : IObserver<string>
        {
            private readonly Action<string> _action;

            public StringDelegateObserver(Action<string> action)
            {
                _action = action;
            }

            public void OnCompleted() { }

            public void OnError(Exception error) { }

            public void OnNext(string value)
            {
                _action?.Invoke(value);
            }
        }
    }
}
