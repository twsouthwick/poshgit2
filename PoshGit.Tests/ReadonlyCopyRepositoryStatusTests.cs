using NSubstitute;
using PoshGit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Reflection;
using System.IO;

namespace PoshGit.Tests
{
    public class ReadonlyCopyRepositoryStatusTests
    {
        [Fact]
        public void AllValuesCopied()
        {
            var mock = Substitute.For<IRepositoryStatus>();

            mock.AheadBy.Returns(1);
            mock.BehindBy.Returns(2);
            mock.Branch.Returns("some-branch");
            mock.CurrentWorkingDirectory.Returns(@"c:\test\asdf\wer");
            mock.GitDir.Returns(@"c:\test\asdf\wer");

            AllPropertiesEqual(mock, new ReadonlyCopyRepositoryStatus(mock));
        }

        [Fact]
        public void UpdatePaths()
        {
            const string path = @"c:\some\test\path";
            var fileCollection = new[] { @"file.txt", @"dir\other.txt" };

            var collection = new ChangedItemsCollection
            {
                Added = fileCollection,
                Modified = fileCollection,
                Deleted = fileCollection,
                Unmerged = fileCollection
            };

            var status = Substitute.For<IRepositoryStatus>();
            status.AheadBy.Returns(1);
            status.BehindBy.Returns(2);
            status.Branch.Returns("some-branch");
            status.CurrentWorkingDirectory.Returns(path);
            status.GitDir.Returns(Path.Combine(path, ".git"));
            status.Index.Returns(collection);
            status.Working.Returns(collection);

            // Test with subdirectory
            var cwd1 = Substitute.For<ICurrentWorkingDirectory>();
            cwd1.CWD.Returns(Path.Combine(path, "dir"));
            cwd1.CreateRelativePath(Path.Combine(path, @"file.txt")).Returns(@"..\file");
            cwd1.CreateRelativePath(Path.Combine(path, @"dir\other.txt")).Returns(@"other.txt");

            var copy1 = new ReadonlyCopyRepositoryStatus(status, cwd1);
            var expected = new[] { @"..\file", @"other.txt" };
            AssertPathsUpdated(cwd1.CWD, expected, copy1);
        }

        private void AssertPathsUpdated(string cwd, IEnumerable<string> expected, IRepositoryStatus status)
        {
            Assert.Equal(cwd, status.CurrentWorkingDirectory);
            AssertPathsUpdated(expected, status.Index);
            AssertPathsUpdated(expected, status.Working);
        }

        private void AssertPathsUpdated(IEnumerable<string> expected, ChangedItemsCollection collection)
        {
            Assert.Equal(expected, collection.Added);
            Assert.Equal(expected, collection.Deleted);
            Assert.Equal(expected, collection.Modified);
            Assert.Equal(expected, collection.Unmerged);
        }

        private void AllPropertiesEqual<T>(T value1, T value2)
        {
            var properties = typeof(T).GetTypeInfo().GetProperties();

            foreach (var property in properties)
            {
                var prop1 = property.GetValue(value1);
                var prop2 = property.GetValue(value2);

                Assert.Equal(prop1, prop2);
            }
        }
    }
}
