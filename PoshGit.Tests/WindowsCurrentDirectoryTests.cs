using NSubstitute;
using PoshGit2;
using Xunit;

namespace PoshGit.Tests
{
    public class WindowsCurrentDirectoryTests
    {
        private static ICurrentWorkingDirectory GetCurrentWorkingDirectory(string cwd)
        {
            var psCwd = Substitute.For<WindowsCurrentWorkingDirectory>();

            psCwd.CWD.Returns(cwd);

            return psCwd;
        }

        [Fact]
        public void SuppliedCurrentDirectory()
        {
            var path = @"c:\some\path\here.txt";
            var cwd = GetCurrentWorkingDirectory(path);

            Assert.Equal(path, cwd.CWD);
        }

        [Fact]
        public void RelativePathConverter()
        {
            var cwd = GetCurrentWorkingDirectory(@"C:\some\path\");

            Assert.Equal(@"..\..\something\here.txt", cwd.CreateRelativePath(@"c:\something\here.txt"));
            Assert.Equal(@".\something\here.txt", cwd.CreateRelativePath(@"c:\some\path\something\here.txt"));
            Assert.Equal(@"..\other\something\here.txt", cwd.CreateRelativePath(@"c:\some\other\something\here.txt"));
        }

        [Fact]
        public void RelativePathConverterNoTrailingSlash()
        {
            var cwd = GetCurrentWorkingDirectory(@"C:\some\path");

            Assert.Equal(@"..\..\something\here.txt", cwd.CreateRelativePath(@"c:\something\here.txt"));
            Assert.Equal(@".\something\here.txt", cwd.CreateRelativePath(@"c:\some\path\something\here.txt"));
            Assert.Equal(@"..\other\something\here.txt", cwd.CreateRelativePath(@"c:\some\other\something\here.txt"));
        }
    }
}
