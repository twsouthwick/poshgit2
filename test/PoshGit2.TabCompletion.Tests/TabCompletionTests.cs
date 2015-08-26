using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PoshGit2.TabCompletion
{
    public class TabCompletionTests
    {
        private readonly ITestOutputHelper _log;

        public TabCompletionTests(ITestOutputHelper log)
        {
            _log = log;
        }

        [InlineData("gi")]
        [InlineData("git")]
        [InlineData("git.")]
        [InlineData("git.exe")]
        [Theory]
        public async Task GitCommand(string cmd)
        {
            var repo = Substitute.For<IRepositoryStatus>();
            var completer = new TabCompleter(Task.FromResult(repo));

            var result = await completer.CompleteAsync(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
        }

        [InlineData("git ", new string[] { "clone", "init" })]
        [Theory]
        public async Task NullStatus(string command, string[] expected)
        {
            var completer = new TabCompleter(Task.FromResult<IRepositoryStatus>(null));
            var fullResult = await completer.CompleteAsync(command, CancellationToken.None);
            var result = GetResult(fullResult);

            Assert.Equal(result, expected.OrderBy(o => o, StringComparer.Ordinal));
        }

        [InlineData("git add ", new string[] { })]
        [InlineData("git rm ", new string[] { })]
        [Theory]
        public async Task EmptyStatus(string command, string[] expected)
        {
            var repo = Substitute.For<IRepositoryStatus>();
            var completer = new TabCompleter(Task.FromResult(repo));
            var fullResult = await completer.CompleteAsync(command, CancellationToken.None);
            var result = GetResult(fullResult);

            Assert.Equal(result, expected.OrderBy(o => o, StringComparer.Ordinal));
        }

        [InlineData("git ", "stash")]
        [InlineData("git s", "stash")]
        [InlineData("git ", "push")]
        [InlineData("git p", "push")]
        [InlineData("git ", "pull")]
        [InlineData("git p", "pull")]
        [InlineData("git ", "bisect")]
        [InlineData("git bis", "bisect")]
        [InlineData("git ", "branch")]
        [InlineData("git br", "branch")]
        [InlineData("git ", "add")]
        [InlineData("git a", "add")]
        [InlineData("git ", "rm")]
        [InlineData("git r", "rm")]
        [InlineData("git ", "merge")]
        [InlineData("git m", "merge")]
        [InlineData("git ", "mergetool")]
        [InlineData("git m", "mergetool")]
        [Theory]
        public async Task ResultContains(string command, string expected)
        {
            var completer = CreateTabCompleter();

            var fullResult = await completer.CompleteAsync(command, CancellationToken.None);
            var result = GetResult(fullResult);

            Assert.Contains(expected, result);
        }

        // Verify command completion
        [InlineData("git ", new[] { "add", "am", "annotate", "archive", "bisect", "blame", "branch", "bundle", "checkout", "cherry", "cherry-pick", "citool", "clean", "clone", "commit", "config", "describe", "diff", "difftool", "fetch", "format-patch", "gc", "grep", "gui", "help", "init", "instaweb", "log", "merge", "mergetool", "mv", "notes", "prune", "pull", "push", "rebase", "reflog", "remote", "rerere", "reset", "revert", "rm", "shortlog", "show", "stash", "status", "submodule", "svn", "tag", "whatchanged" })]
        [InlineData("git.exe ", new[] { "add", "am", "annotate", "archive", "bisect", "blame", "branch", "bundle", "checkout", "cherry", "cherry-pick", "citool", "clean", "clone", "commit", "config", "describe", "diff", "difftool", "fetch", "format-patch", "gc", "grep", "gui", "help", "init", "instaweb", "log", "merge", "mergetool", "mv", "notes", "prune", "pull", "push", "rebase", "reflog", "remote", "rerere", "reset", "revert", "rm", "shortlog", "show", "stash", "status", "submodule", "svn", "tag", "whatchanged" })]

        // git add
        [InlineData("git add ", new[] { "working-duplicate", "working-modified", "working-added", "working-unmerged" })]
        [InlineData("git add working-m", new[] { "working-modified" })]

        // git rm
        [InlineData("git rm ", new[] { "working-deleted", "working-duplicate" })]
        [InlineData("git rm working-a", new string[] { })]
        [InlineData("git rm working-d", new string[] { "working-deleted", "working-duplicate" })]

        // git bisect
        [InlineData("git bisect ", new[] { "start", "bad", "good", "skip", "reset", "visualize", "replay", "log", "run" })]
        [InlineData("git bisect s", new[] { "start", "skip" })]
        [InlineData("git bisect bad ", new string[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git bisect good ", new string[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git bisect reset ", new string[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git bisect skip ", new string[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git bisect bad f", new string[] { "feature1", "feature2" })]
        [InlineData("git bisect good f", new string[] { "feature1", "feature2" })]
        [InlineData("git bisect reset f", new string[] { "feature1", "feature2" })]
        [InlineData("git bisect skip f", new string[] { "feature1", "feature2" })]
        [InlineData("git bisect bad g", new string[] { })]
        [InlineData("git bisect good g", new string[] { })]
        [InlineData("git bisect reset g", new string[] { })]
        [InlineData("git bisect skip g", new string[] { })]
        [InlineData("git bisect skip H", new string[] { "HEAD" })]

        // git notes
        [InlineData("git notes ", new[] { "edit", "show" })]
        [InlineData("git notes e", new[] { "edit" })]

        // git reflog
        [InlineData("git reflog ", new[] { "expire", "delete", "show" })]
        [InlineData("git reflog e", new[] { "expire" })]

        // git branch
        [InlineData("git branch -d ", new string[] { "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git branch -D ", new string[] { "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git branch -m ", new string[] { "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git branch -M ", new string[] { "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git branch -d f", new string[] { "feature1", "feature2" })]
        [InlineData("git branch -D f", new string[] { "feature1", "feature2" })]
        [InlineData("git branch -m f", new string[] { "feature1", "feature2" })]
        [InlineData("git branch -M f", new string[] { "feature1", "feature2" })]
        [InlineData("git branch -d g", new string[] { })]
        [InlineData("git branch -D g", new string[] { })]
        [InlineData("git branch -m g", new string[] { })]
        [InlineData("git branch -M g", new string[] { })]
        [InlineData("git branch newBranch ", new string[] { "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git branch newBranch f", new string[] { "feature1", "feature2" })]
        [InlineData("git branch newBranch g", new string[] { })]

        // git push
        [InlineData("git push ", new string[] { "origin", "other" })]
        [InlineData("git push oth", new string[] { "other" })]
        [InlineData("git push origin ", new string[] { "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git push origin fe", new string[] { "feature1", "feature2" })]
        [InlineData("git push origin :", new string[] { ":remotefeature", ":cutfeature" })]
        [InlineData("git push origin :re", new string[] { ":remotefeature" })]

        // git pull
        [InlineData("git pull ", new string[] { "origin", "other" })]
        [InlineData("git pull oth", new string[] { "other" })]
        [InlineData("git pull origin ", new string[] { "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git pull origin fe", new string[] { "feature1", "feature2" })]

        // git fetch
        [InlineData("git fetch ", new string[] { "origin", "other" })]
        [InlineData("git fetch oth", new string[] { "other" })]

        // git submodule
        [InlineData("git submodule ", new string[] { "add", "status", "init", "update", "summary", "foreach", "sync" })]
        [InlineData("git submodule s", new string[] { "status", "summary", "sync" })]

        // git svn
        [InlineData("git svn ", new string[] { "init", "fetch", "clone", "rebase", "dcommit", "branch", "tag", "log", "blame", "find-rev", "set-tree", "create-ignore", "show-ignore", "mkdirs", "commit-diff", "info", "proplist", "propget", "show-externals", "gc", "reset" })]
        [InlineData("git svn f", new string[] { "fetch", "find-rev" })]

        // git stash
        [InlineData("git stash ", new string[] { "list", "save", "show", "drop", "pop", "apply", "branch", "clear", "create" })]
        [InlineData("git stash s", new string[] { "save", "show" })]
        [InlineData("git stash show ", new string[] { "stash", "wip" })]
        [InlineData("git stash show w", new string[] { "wip" })]
        [InlineData("git stash show d", new string[] { })]
        [InlineData("git stash apply ", new string[] { "stash", "wip" })]
        [InlineData("git stash apply w", new string[] { "wip" })]
        [InlineData("git stash apply d", new string[] { })]
        [InlineData("git stash drop ", new string[] { "stash", "wip" })]
        [InlineData("git stash drop w", new string[] { "wip" })]
        [InlineData("git stash drop d", new string[] { })]
        [InlineData("git stash pop ", new string[] { "stash", "wip" })]
        [InlineData("git stash pop w", new string[] { "wip" })]
        [InlineData("git stash pop d", new string[] { })]
        [InlineData("git stash branch ", new string[] { "stash", "wip" })]
        [InlineData("git stash branch w", new string[] { "wip" })]
        [InlineData("git stash branch d", new string[] { })]

        // Tests for commit
        [InlineData("git commit -C ", new string[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git commit -C O", new string[] { "ORIG_HEAD" })]
        [InlineData("git commit -C o", new string[] { "origin/cutfeature", "origin/remotefeature" })]

        // git remote
        [InlineData("git remote ", new[] { "add", "rename", "rm", "set-head", "show", "prune", "update" })]
        [InlineData("git remote r", new[] { "rename", "rm" })]
        [InlineData("git remote rename ", new string[] { "origin", "other" })]
        [InlineData("git remote rename or", new string[] { "origin" })]
        [InlineData("git remote rm ", new string[] { "origin", "other" })]
        [InlineData("git remote rm or", new string[] { "origin" })]
        [InlineData("git remote set-head ", new string[] { "origin", "other" })]
        [InlineData("git remote set-head or", new string[] { "origin" })]
        [InlineData("git remote set-branches ", new string[] { "origin", "other" })]
        [InlineData("git remote set-branches or", new string[] { "origin" })]
        [InlineData("git remote set-url ", new string[] { "origin", "other" })]
        [InlineData("git remote set-url or", new string[] { "origin" })]
        [InlineData("git remote show ", new string[] { "origin", "other" })]
        [InlineData("git remote show or", new string[] { "origin", })]
        [InlineData("git remote prune ", new string[] { "origin", "other" })]
        [InlineData("git remote prune or", new string[] { "origin" })]

        // git help <cmd>
        [InlineData("git help ", new[] { "add", "am", "annotate", "archive", "bisect", "blame", "branch", "bundle", "checkout", "cherry", "cherry-pick", "citool", "clean", "clone", "commit", "config", "describe", "diff", "difftool", "fetch", "format-patch", "gc", "grep", "gui", "help", "init", "instaweb", "log", "merge", "mergetool", "mv", "notes", "prune", "pull", "push", "rebase", "reflog", "remote", "rerere", "reset", "revert", "rm", "shortlog", "show", "stash", "status", "submodule", "svn", "tag", "whatchanged" })]
        [InlineData("git help ch", new[] { "checkout", "cherry", "cherry-pick" })]

        // git checkout -- <files>
        [InlineData("git checkout -- ", new[] { "working-deleted", "working-duplicate", "working-modified", "working-unmerged" })]
        [InlineData("git checkout -- working-d", new[] { "working-deleted", "working-duplicate" })]

        // git merge|mergetool <files>
        // TODO: Enable for merge state
        //[InlineData("git merge ", new[] { "working-unmerged", "working-duplicate" })]
        //[InlineData("git merge working-u", new[] { "working-unmerged" })]
        //[InlineData("git merge j", new string[] { })]
        //[InlineData("git mergetool ", new[] { "working-unmerged", "working-duplicate" })]
        //[InlineData("git mergetool working-u", new[] { "working-unmerged" })]
        //[InlineData("git mergetool j", new string[] { })]

        // git checkout <branch>
        [InlineData("git checkout ", new[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]

        // git cherry-pick <branch>
        [InlineData("git cherry ", new[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]

        // git cherry-pick <branch>
        [InlineData("git cherry-pick ", new[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]

        // git diff <branch>
        [InlineData("git diff ", new[] { "index-modified", "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git diff --cached ", new[] { "working-modified", "working-duplicate", "working-unmerged", "index-modified", "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git diff --staged ", new[] { "index-modified", "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]

        // git difftool <branch>
        [InlineData("git difftool ", new[] { "index-modified", "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git difftool --cached ", new[] { "working-modified", "working-duplicate", "working-unmerged", "index-modified", "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git difftool --staged ", new[] { "index-modified", "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]

        // git log <branch>
        [InlineData("git log ", new[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]

        // git merge <branch>
        [InlineData("git merge ", new[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]

        // git rebase <branch>
        [InlineData("git rebase ", new[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]

        // git reflog <branch>
        [InlineData("git reflog show ", new[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]

        // git reset <branch>
        [InlineData("git reset ", new[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]

        // git reset HEAD <file>
        [InlineData("git reset HEAD ", new[] { "index-added", "index-deleted", "index-modified", "index-unmerged" })]
        [InlineData("git reset HEAD index-a", new[] { "index-added" })]

        // git revert <branch>
        [InlineData("git revert ", new[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]

        // git show<branch>
        [InlineData("git show ", new[] { "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]

        [Theory]
        public async Task CheckCompletion(string cmd, string[] expected)
        {
            var completer = CreateTabCompleter();

            await CompareAsync(completer, cmd, expected.OrderBy(o => o, StringComparer.Ordinal));
        }

        // git add
        [InlineData("git add ", new[] { "working duplicate", "working modified", "working added", "working unmerged" })]
        [InlineData("git add \"working m", new[] { "working modified" })]
        [InlineData("git add \'working m", new[] { "working modified" })]

        // git rm
        [InlineData("git rm ", new[] { "working deleted", "working duplicate" })]
        [InlineData("git rm \"working d", new string[] { "working deleted", "working duplicate" })]
        [InlineData("git rm \'working d", new string[] { "working deleted", "working duplicate" })]

        // git checkout -- <files>
        [InlineData("git checkout -- ", new[] { "working deleted", "working duplicate", "working modified", "working unmerged" })]
        [InlineData("git checkout -- \"wor", new[] { "working deleted", "working duplicate", "working modified", "working unmerged" })]
        [InlineData("git checkout -- \"working d", new[] { "working deleted", "working duplicate" })]
        [InlineData("git checkout -- \'working d", new[] { "working deleted", "working duplicate" })]

        // git merge|mergetool <files>
        // TODO: Enable for merge state
        //[InlineData("git merge ", new[] { "working unmerged", "working duplicate" })]
        //[InlineData("git merge working u", new[] { "working unmerged" })]
        //[InlineData("git merge j", new string[] { })]
        //[InlineData("git mergetool ", new[] { "working unmerged", "working duplicate" })]
        //[InlineData("git mergetool working u", new[] { "working unmerged" })]
        //[InlineData("git mergetool j", new string[] { })]

        // git diff <branch>
        [InlineData("git diff ", new[] { "index modified", "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git diff --cached ", new[] { "working modified", "working duplicate", "working unmerged", "index modified", "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git diff --staged ", new[] { "index modified", "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]

        // git difftool <branch>
        [InlineData("git difftool ", new[] { "index modified", "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git difftool --cached ", new[] { "working modified", "working duplicate", "working unmerged", "index modified", "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]
        [InlineData("git difftool --staged ", new[] { "index modified", "FETCH_HEAD", "HEAD", "MERGE_HEAD", "ORIG_HEAD", "feature1", "feature2", "master", "origin/cutfeature", "origin/remotefeature" })]

        // git reset HEAD <file>
        [InlineData("git reset HEAD ", new[] { "index added", "index deleted", "index modified", "index unmerged" })]
        [InlineData("git reset HEAD \"index a", new[] { "index added" })]
        [InlineData("git reset HEAD \'index a", new[] { "index added" })]

        [Theory]
        public async Task CheckCompletionWithQuotations(string cmd, string[] initialExpected)
        {
            const string quot = "\"";
            var completer = CreateTabCompleter(" ");
            var expected = initialExpected
                .OrderBy(o => o, StringComparer.Ordinal)
                .Select(o => o.Contains(" ") ? $"{quot}{o}{quot}" : o);

            await CompareAsync(completer, cmd, expected);
        }

        private async Task CompareAsync(ITabCompleter completer, string cmd, IEnumerable<string> expected)
        {
            var fullResult = await completer.CompleteAsync(cmd, CancellationToken.None);
            var result = GetResult(fullResult);

            _log.WriteLine("Expected output:");
            _log.WriteLine(string.Join(Environment.NewLine, expected));
            _log.WriteLine(string.Empty);
            _log.WriteLine("Actual output:");
            _log.WriteLine(string.Join(Environment.NewLine, result));

            Assert.Equal(expected, result);
        }

        private static ITabCompleter CreateTabCompleter(string join = "-")
        {
            var status = Substitute.For<IRepositoryStatus>();

            var working = new ChangedItemsCollection
            {
                Added = new[] { $"working{join}added", $"working{join}duplicate" },
                Deleted = new[] { $"working{join}deleted", $"working{join}duplicate" },
                Modified = new[] { $"working{join}modified", $"working{join}duplicate" },
                Unmerged = new[] { $"working{join}unmerged", $"working{join}duplicate" }
            };

            var index = new ChangedItemsCollection
            {
                Added = new[] { $"index{join}added" },
                Deleted = new[] { $"index{join}deleted" },
                Modified = new[] { $"index{join}modified" },
                Unmerged = new[] { $"index{join}unmerged" }
            };

            status.Index.Returns(index);
            status.Working.Returns(working);

            status.LocalBranches.Returns(new[] { "master", "feature1", "feature2" });
            status.Remotes.Returns(new[] { "origin", "other" });
            status.RemoteBranches.Returns(new[] { "origin/remotefeature", "origin/cutfeature" });
            status.Stashes.Returns(new[] { "stash", "wip" });

            return new TabCompleter(Task.FromResult(status));
        }

        private IEnumerable<string> GetResult(TabCompletionResult fullResult)
        {
            Assert.True(fullResult.IsSuccess);

            return (fullResult as TabCompletionResult.Success).Item;
        }
    }
}
