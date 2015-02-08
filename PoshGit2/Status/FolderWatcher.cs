using System;
using System.Diagnostics;
using System.IO;

namespace PoshGit2
{
    public class FolderWatcher : FileSystemWatcherObserver, IFolderWatcher
    {
        private readonly IFileWatcher _gitlockWatcher;

        public FolderWatcher(string folder, Func<string, IFileWatcher> fileWatcherFactory)
            : base(folder, string.Empty, true)
        {
            _gitlockWatcher = fileWatcherFactory($"{Path.Combine(folder, ".git")}index.lock");
            _gitlockWatcher.Subscribe(new DelegateObserver(GitLockChanged));
        }

        private void GitLockChanged(FileChangedStatus lock_status)
        {
            Trace.Write($@".git\index.lock [{lock_status}]");

            switch (lock_status)
            {
                case FileChangedStatus.Created:
                    Pause();
                    break;
                case FileChangedStatus.Deleted:
                    Start();
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                (_gitlockWatcher as IDisposable)?.Dispose();
            }
        }
    }
}