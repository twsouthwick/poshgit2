using System.IO;

namespace PoshGit2
{
    public class FileWatcher : FileSystemWatcherObserver
    {
        public FileWatcher(string file)
            : base(Path.GetDirectoryName(file), Path.GetFileName(file), false)
        { }
    }
}