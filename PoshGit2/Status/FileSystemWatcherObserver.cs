using System;
using System.Diagnostics;
using System.IO;

namespace PoshGit2
{
    public abstract class FileSystemWatcherObserver : ObservableBase<FileChangedStatus>, IDisposable, IFileWatcher
    {
        private readonly FileSystemWatcher _filesystemWatcher;

        protected FileSystemWatcherObserver(string folder, string filter, bool includeSubDirectories)
        {
            _filesystemWatcher = new FileSystemWatcher(folder)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = includeSubDirectories,
                Filter = filter,
                NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Security
            };

            _filesystemWatcher.Changed += FilesChanged;
            _filesystemWatcher.Created += FilesChanged;
            _filesystemWatcher.Deleted += FilesChanged;
            _filesystemWatcher.Renamed += FilesChanged;
        }

        public void Pause()
        {
            _filesystemWatcher.EnableRaisingEvents = false;
        }

        public void Start()
        {
            _filesystemWatcher.EnableRaisingEvents = true;
        }

        private void FilesChanged(object sender, FileSystemEventArgs e)
        {
            Trace.WriteLine($"File changed [{e.ChangeType}]: {e.Name}");

            if (e.ChangeType.HasFlag(WatcherChangeTypes.Created))
            {
                OnNext(FileChangedStatus.Created);
            }
            if (e.ChangeType.HasFlag(WatcherChangeTypes.Deleted))
            {
                OnNext(FileChangedStatus.Deleted);
            }
            else
            {
                OnNext(FileChangedStatus.Changed);
            }
        }

        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _filesystemWatcher.EnableRaisingEvents = false;

                    _filesystemWatcher.Changed -= FilesChanged;
                    _filesystemWatcher.Created -= FilesChanged;
                    _filesystemWatcher.Deleted -= FilesChanged;
                    _filesystemWatcher.Renamed -= FilesChanged;

                    _filesystemWatcher.Dispose();
                }


                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}