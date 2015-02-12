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
            var observer = Substitute.For<IObserver<string>>();

            watcher.Subscribe(observer);

            var newFile = Path.Combine(directory, "rewrite-file.tmp");
            File.WriteAllText(newFile, "test");
            File.WriteAllText(newFile, "something");
            File.Delete(newFile);

            await Task.Delay(DelayTime);

            // Verify 4 calls were made for newFile only
            observer.Received(4).OnNext(Arg.Any<string>());
            observer.Received(4).OnNext(newFile);
        }

        [Fact]
        public async Task WatchSubFolder()
        {
            var directory = CreateTempGitDirectory();
            var subdirectory = Path.Combine(directory, "subdir");
            Directory.CreateDirectory(subdirectory);

            var watcher = new GitFolderWatcher(directory);
            var observer = Substitute.For<IObserver<string>>();

            watcher.Subscribe(observer);

            var newFile = Path.Combine(subdirectory, "write-update-delete.tmp");
            File.WriteAllText(newFile, "test");
            File.WriteAllText(newFile, "something");
            File.Delete(newFile);

            await Task.Delay(DelayTime);

            // Verify expected calls were made.  The subdirectory notification is not necessary, but occurs
            observer.Received(5).OnNext(Arg.Any<string>());
            observer.Received(4).OnNext(newFile);
            observer.Received(1).OnNext(subdirectory);
        }

        [Fact]
        public async Task DoNotWatchGitFolder()
        {
            var directory = CreateTempGitDirectory();
            var watcher = new GitFolderWatcher(directory);
            var observer = Substitute.For<IObserver<string>>();

            watcher.Subscribe(observer);

            var newFile = Path.Combine(directory, ".git", "write-update-delete.tmp");
            File.WriteAllText(newFile, "test");
            File.WriteAllText(newFile, "something");
            File.Delete(newFile);

            await Task.Delay(DelayTime);

            observer.Received(0).OnNext(Arg.Any<string>());
        }

        [Fact]
        public async Task WatchGitLockFile()
        {
            var directory = CreateTempGitDirectory();
            var watcher = new GitFolderWatcher(directory);
            var observer = Substitute.For<IObserver<string>>();

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

            // Verify that only two file changes were detected (file2.tmp and git lock removed)
            observer.Received(2).OnNext(Arg.Any<string>());
            observer.Received(1).OnNext(file2);
            observer.Received(1).OnNext(lockfile);
        }

        [Fact]
        public async Task WatchSubLockFile()
        {
            var directory = CreateTempGitDirectory();
            var watcher = new GitFolderWatcher(directory);
            var observer = Substitute.For<IObserver<string>>();

            var subdir = Path.Combine(directory, ".git", "somefolder");
            Directory.CreateDirectory(subdir);

            watcher.Subscribe(observer);

            // Simulate git lock
            var lockfile = Path.Combine(subdir, "other.lock");
            File.WriteAllText(lockfile, "");

            // Remove lock file
            File.Delete(lockfile);

            // Give the filewatchers some time to react
            await Task.Delay(DelayTime);

            // Verify that only two file changes were detected (file2.tmp and git lock removed)
            observer.Received(1).OnNext(Arg.Any<string>());
            observer.Received(1).OnNext(lockfile);
        }

        [Fact]
        public async Task SubscriberNotifiedWhenGitLockRemoved()
        {
            var directory = CreateTempGitDirectory();
            var watcher = new GitFolderWatcher(directory);
            var observer = Substitute.For<IObserver<string>>();

            watcher.Subscribe(observer);

            // Simulate git lock
            var lockfile = Path.Combine(directory, ".git", "index.lock");
            File.WriteAllText(lockfile, "");

            // Create file now that git is locked
            var file1 = Path.Combine(directory, "file1.tmp");
            File.WriteAllText(file1, "contents");

            // Remove lock file
            File.Delete(lockfile);

            await Task.Delay(DelayTime);

            // Verify that only one file change was detected
            observer.Received(1).OnNext(Arg.Any<string>());
            observer.Received(1).OnNext(lockfile);
        }
    }
}
