using NSubstitute;
using PoshGit2;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace PoshGit.Tests
{
    public class GitFolderWatcherTests
    {
        private const int DelayTime = 10;

        private static string CreateTempGitDirectory()
        {
            var testFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(testFolder);
            Directory.CreateDirectory(Path.Combine(testFolder, ".git"));

            Console.WriteLine(testFolder);

            return testFolder;
        }

        [Fact]
        public async Task WatchFolder()
        {
            var directory = CreateTempGitDirectory();
            var watcher = new GitFolderWatcher(directory);
            var observer = Substitute.For<IObserver<FileChangedStatus>>();

            watcher.Subscribe(observer);

            var newFile = Path.Combine(directory, "rewrite-file.tmp");
            File.WriteAllText(newFile, "test");
            File.WriteAllText(newFile, "something");
            File.Delete(newFile);

            await Task.Delay(DelayTime);

            observer.Received(3).OnNext(FileChangedStatus.Changed);
        }

        [Fact]
        public async Task WatchSubFolder()
        {
            var directory = CreateTempGitDirectory();
            var subdirectory = Path.Combine(directory, "subdir");
            Directory.CreateDirectory(subdirectory);

            var watcher = new GitFolderWatcher(directory);
            var observer = Substitute.For<IObserver<FileChangedStatus>>();

            watcher.Subscribe(observer);

            var newFile = Path.Combine(subdirectory, "write-update-delete.tmp");
            File.WriteAllText(newFile, "test");
            File.WriteAllText(newFile, "something");
            File.Delete(newFile);

            await Task.Delay(DelayTime);

            // Should be three, but because it's in a subdirectory it is four
            observer.Received(4).OnNext(FileChangedStatus.Changed);
        }

        [Fact]
        public async Task DoNotWatchGitFolder()
        {
            var directory = CreateTempGitDirectory();
            var watcher = new GitFolderWatcher(directory);
            var observer = Substitute.For<IObserver<FileChangedStatus>>();

            watcher.Subscribe(observer);

            var newFile = Path.Combine(directory, ".git", "write-update-delete.tmp");
            File.WriteAllText(newFile, "test");
            File.WriteAllText(newFile, "something");
            File.Delete(newFile);

            await Task.Delay(DelayTime);

            observer.Received(0).OnNext(FileChangedStatus.Changed);
        }

        [Fact]
        public async Task WatchGitLockFile()
        {
            var directory = CreateTempGitDirectory();
            var watcher = new GitFolderWatcher(directory);
            var observer = Substitute.For<IObserver<FileChangedStatus>>();

            watcher.Subscribe(observer);

            // Simulate git lock
            var lockfile = Path.Combine(directory, ".git", "index.lock");
            File.WriteAllText(lockfile, "");

            // Create file now that git is locked
            var file1 = Path.Combine(directory, "file1.tmp");
            File.WriteAllText(file1, "contents");

            // Remove lock file
            File.Delete(lockfile);

            // Create file
            var file2 = Path.Combine(directory, "file2.tmp");
            File.WriteAllText(file2, "contents");

            // Give the filewatchers some time to react
            await Task.Delay(DelayTime);

            // Verify that only one file change was detected
            observer.Received(1).OnNext(FileChangedStatus.Changed);
        }
    }
}
