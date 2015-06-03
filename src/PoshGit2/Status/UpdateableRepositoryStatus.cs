using LibGit2Sharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public sealed class UpdateableRepositoryStatus : IDisposable, IRepositoryStatus
    {
        private const int MaxUpdateRepoCount = 2;
        private const string Elipses = "...";

        private readonly ICurrentWorkingDirectory _cwd;
        private readonly ILogger _log;
        private readonly SemaphoreSlim _updateGateSemaphore = new SemaphoreSlim(MaxUpdateRepoCount, MaxUpdateRepoCount);
        private readonly SemaphoreSlim _updateRepoSemaphore = new SemaphoreSlim(1, 1);
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        // These are set in an initialization method so that it does not stall the console output
        private IRepository _repository;
        private IFolderWatcher _folderWatcher;

        public UpdateableRepositoryStatus(string folder, ILogger log, Func<string, IRepository> repositoryFactory,
            Func<string, IFolderWatcher> folderWatcherFactory, ICurrentWorkingDirectory cwd)
        {
            _log = log;
            _cwd = cwd;

            Working = new ChangedItemsCollection();
            Index = new ChangedItemsCollection();

            Task.Run(() => Initialize(folder, repositoryFactory, folderWatcherFactory));
        }

        private void Initialize(string folder, Func<string, IRepository> repositoryFactory,
            Func<string, IFolderWatcher> folderWatcherFactory)
        {
            _repository = repositoryFactory(folder);

            _folderWatcher = folderWatcherFactory(folder);
            _folderWatcher.OnNext += UpdateStatus;

            // _repository.Info.Path returns a path ending with '\'
            GitDir = _repository.Info.Path.Substring(0, _repository.Info.Path.Length - 1);
            CurrentWorkingDirectory = Path.GetFullPath(Path.Combine(GitDir, ".."));

            UpdateStatus(CurrentWorkingDirectory);
        }

        private bool IsUpdating => _updateGateSemaphore.CurrentCount != MaxUpdateRepoCount;

        public string Branch => IsUpdating ? $"{ExpandBranchName()}{Elipses}" : ExpandBranchName();

        public ChangedItemsCollection Working { get; set; }

        public ChangedItemsCollection Index { get; set; }

        public string GitDir { get; set; }

        public string CurrentWorkingDirectory { get; set; }

        public int AheadBy => _repository?.Head.TrackingDetails.AheadBy ?? 0;

        public int BehindBy => _repository?.Head.TrackingDetails.BehindBy ?? 0;

        public IReadOnlyCollection<string> LocalBranches => new ReadonlyEnumerableCollection<string>(_repository?.Branches.Where(b => !b.IsRemote).Select(b => b.Name) ?? Enumerable.Empty<string>());

        public IReadOnlyCollection<string> RemoteBranches => new ReadonlyEnumerableCollection<string>(_repository?.Branches.Where(b => b.IsRemote).Select(b => b.Name) ?? Enumerable.Empty<string>());

        public IReadOnlyCollection<string> Stashes => new ReadonlyEnumerableCollection<string>(_repository?.Stashes.Select(s => s.Name) ?? Enumerable.Empty<string>());

        public IReadOnlyCollection<string> Remotes => new ReadonlyEnumerableCollection<string>(_repository?.Network.Remotes.Select(r => r.Name) ?? Enumerable.Empty<string>());

        public IReadOnlyCollection<ConfigurationEntry<string>> Configuration => new ReadonlyEnumerableCollection<ConfigurationEntry<string>>(_repository?.Config ?? Enumerable.Empty<ConfigurationEntry<string>>());

        /// <summary>
        /// Update the status from a file change asynchronously 
        /// 
        /// This gates it so that only one update occurs at a time and at most one update
        /// is pending.  This is so that when a large number of file updates are triggered,
        /// we make sure to catch the final one.
        /// </summary>
        /// <param name="file"></param>
        public async void UpdateStatus(string file)
        {
            _log.Debug("File {File} was modified", file);

            if (!_updateGateSemaphore.Wait(0))
            {
                return;
            }

            try
            {
                await _updateRepoSemaphore.WaitAsync(_cancellationTokenSource.Token);

                _log.Information("Update was triggered by {File}", file);

                UpdateStatus();

                // Force a 1000ms delay between processing in cases when large number of events
                // are seen, such as removing a large number of files
                await Task.Delay(1000, _cancellationTokenSource.Token);

                return;
            }
            catch (OperationCanceledException) { }
            finally
            {
                _updateRepoSemaphore.Release();
                _updateGateSemaphore.Release();
            }
        }

        private void UpdateStatus()
        {
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
                _log.Error(e, "Unexpected git exception");
            }
            catch (Exception e)
            {
                _log.Error(e, "Unexpected exception");
            }
        }

        /// <summary>
        /// Expand branch name to give description such as MERGING, REBASE, CHERRY-PICKING, etc
        /// </summary>
        /// <returns></returns>
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

        private IReadOnlyCollection<string> GetCollection(params IEnumerable<StatusEntry>[] entries)
        {
            return new ReadonlyEnumerableCollection<string>(entries.SelectMany(o => o).Select(f => f.FilePath));
        }

        public void Dispose()
        {
            _repository.Dispose();

            _folderWatcher.OnNext -= UpdateStatus;
            (_folderWatcher as IDisposable)?.Dispose();

            _updateRepoSemaphore.Dispose();
            _updateGateSemaphore.Dispose();

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        private class ReadonlyEnumerableCollection<T> : IReadOnlyCollection<T>
        {
            private readonly ICollection<T> _collection;

            public ReadonlyEnumerableCollection(IEnumerable<T> enumerable)
            {
                _collection = enumerable.ToList();
                Count = _collection.Count;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _collection.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_collection).GetEnumerator();
            }

            public int Count { get; }
        }
    }
}
