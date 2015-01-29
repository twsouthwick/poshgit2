using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoshGit2
{
    public sealed class UpdateableRepositoryStatus : IDisposable, IRepositoryStatus
    {
        private readonly IRepository _repository;
        private readonly IFileWatcher _fileWatcher;

        public UpdateableRepositoryStatus(string folder, Func<string, IRepository> repositoryFactory, Func<string, IFolderWatcher> folderWatcherFactory)
        {
            _repository = repositoryFactory(folder);

            _fileWatcher = folderWatcherFactory(folder);
            _fileWatcher.Subscribe(new DelegateObserver(_ => UpdateStatus()));

            GitDir = _repository.Info.Path;

            Working = new ChangedItemsCollection();
            Index = new ChangedItemsCollection();

            UpdateStatus();
        }

        public string Branch { get { return _repository.Head.Name; } }
        public bool HasWorking { get { return Working.HasAny; } }
        public ChangedItemsCollection Working { get; set; }
        public bool HasUntracked { get; set; }
        public ChangedItemsCollection Index { get; set; }
        public bool HasIndex { get { return Index.HasAny; } }
        public string GitDir { get; }
        public int AheadBy { get { return _repository.Head.TrackingDetails.AheadBy ?? 0; } }
        public int BehindBy { get { return _repository.Head.TrackingDetails.BehindBy ?? 0; } }

        public void UpdateStatus()
        {
            _fileWatcher.Pause();

            var repositoryStatus = _repository.RetrieveStatus();

            Working = new ChangedItemsCollection
            {
                Added = GetCollection(repositoryStatus.Untracked),
                Deleted = GetCollection(repositoryStatus.Missing),
                Modified = GetCollection(repositoryStatus.Modified)
            };

            Index = new ChangedItemsCollection
            {
                Added = GetCollection(repositoryStatus.Added),
                Deleted = GetCollection(repositoryStatus.Removed),
                Modified = GetCollection(repositoryStatus.Staged)
            };

            _fileWatcher.Start();
        }

        private ICollection<string> GetCollection(IEnumerable<StatusEntry> entries)
        {
            return entries.Select(f => f.FilePath).ToList().AsReadOnly();
        }

        public void Dispose()
        {
            _repository.Dispose();
            (_fileWatcher as IDisposable)?.Dispose();
        }
    }
}
