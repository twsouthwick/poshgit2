using System;
using System.Diagnostics;
using System.IO;

namespace PoshGit2
{
    public sealed class GitFolderWatcher : ObservableBase<FileChangedStatus>, IFolderWatcher, IDisposable
    {
        private readonly string _folder;
        private readonly string _gitdir;
        private readonly FileSystemWatcher _workingDirectoryWatcher;
        private readonly FileSystemWatcher _gitlockWatcher;

        public GitFolderWatcher(string folder)
        {
            _folder = folder;
            _gitdir = Path.Combine(folder, ".git");

            _workingDirectoryWatcher = SetupWorkingDirectoryWatcher(folder);
            _gitlockWatcher = SetupLockWatcher(_gitdir);
        }

        private FileSystemWatcher SetupLockWatcher(string gitdir)
        {
            var filewatcher = new FileSystemWatcher(gitdir)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
                Filter = "index.lock",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };

            filewatcher.Created += GitLockCreated;
            filewatcher.Deleted += GitLockDeleted;

            return filewatcher;
        }

        private void GitLockDeleted(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine("Git lock deleted");
            _workingDirectoryWatcher.EnableRaisingEvents = true;
            OnNext(FileChangedStatus.Changed);
        }

        private void GitLockCreated(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine("Git lock created");
            _workingDirectoryWatcher.EnableRaisingEvents = false;
        }

        private FileSystemWatcher SetupWorkingDirectoryWatcher(string directory)
        {
            var filewatcher = new FileSystemWatcher(directory)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };

            filewatcher.Changed += FileChanged;

            return filewatcher;
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.StartsWith(_gitdir, StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.WriteLine($"Skipping file: {e.FullPath}");
                return;
            }

            Debug.WriteLine($"Processing file: {e.FullPath}");
            OnNext(FileChangedStatus.Changed);
        }

        public void Dispose()
        {
            _workingDirectoryWatcher.Dispose();
            _gitlockWatcher.Dispose();
        }
    }
}