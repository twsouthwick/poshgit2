using System;
using System.Diagnostics;
using System.IO;

namespace PoshGit2
{
    public sealed class GitFolderWatcher : IFolderWatcher, IDisposable
    {
        private readonly string _folder;
        private readonly string _gitdir;
        private readonly FileSystemWatcher _workingDirectoryWatcher;
#if FILTER_GITFILES
        private readonly FileSystemWatcher _gitlockWatcher;
#endif

        public GitFolderWatcher(string folder)
        {
            _folder = folder;
            _gitdir = Path.Combine(folder, ".git");

            _workingDirectoryWatcher = SetupWorkingDirectoryWatcher(folder);
#if FILTER_GITFILES
            _gitlockWatcher = SetupLockWatcher(_gitdir);
#endif
        }

#if FILTER_GIT_FILES
        private FileSystemWatcher SetupLockWatcher(string gitdir)
        {
            var filewatcher = new FileSystemWatcher(gitdir)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                Filter = "*.lock",
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
            OnNext?.Invoke(e.FullPath);
        }

        private void GitLockCreated(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine("Git lock created");
            _workingDirectoryWatcher.EnableRaisingEvents = false;
        }
#endif

        private FileSystemWatcher SetupWorkingDirectoryWatcher(string directory)
        {
            var filewatcher = new FileSystemWatcher(directory)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };

            filewatcher.Changed += FileChanged;
            filewatcher.Deleted += FileChanged;

            return filewatcher;
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
#if FILTER_GIT_FILES
            if (e.FullPath.StartsWith(_gitdir, StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.WriteLine($"Skipping file: {e.FullPath}");
                return;
            }
#endif

            Debug.WriteLine($"Processing file: {e.FullPath}");
            OnNext?.Invoke(e.FullPath);
        }

        public event Action<string> OnNext;

        public void Dispose()
        {
            _workingDirectoryWatcher.Dispose();
#if FILTER_GIT_FILES
            _gitlockWatcher.Dispose();
#endif
        }
    }
}