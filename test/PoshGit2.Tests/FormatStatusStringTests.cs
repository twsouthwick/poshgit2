using PoshGit2;
using Xunit;

namespace PoshGit.Tests
{
    public class FormatStatusStringTests
    {
        [Fact]
        public void SimpleBranchTest()
        {
            var formatter = new FormatStatusString();
            var status = new ReadWriteRepositoryStatus
            {
                Branch = "test-branch"
            };

            Assert.Equal("Hello test-branch", formatter.Format("Hello {Branch}", status));
        }

        [Fact]
        public void SubObjects()
        {
            var formatter = new FormatStatusString();
            var status = new ReadWriteRepositoryStatus
            {
                Working = new ChangedItemsCollection
                {
                    Added = new[] { "hello1", "hello2" }
                }
            };

            Assert.Equal("Hello 2", formatter.Format("Hello {Working.Added.Length}", status));
        }
    }
}
