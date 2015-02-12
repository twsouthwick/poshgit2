using PoshGit2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PoshGit.Tests
{
    public class WindowsCurrentDirectoryTests
    {
        [Fact]
        public void CurrentDirectory()
        {
            var cwd = new WindowsCurrentDirectory();

            Assert.Equal(Environment.CurrentDirectory, cwd.CWD);
        }

        [Fact]
        public void SuppliedCurrentDirectory()
        {
            var path = @"c:\some\path\here.txt";
            var cwd = new WindowsCurrentDirectory(path);

            Assert.Equal(path, cwd.CWD);
        }

        [Fact]
        public void RelativePathConverter()
        {
            var cwd = new WindowsCurrentDirectory(@"C:\some\path\");

            Assert.Equal(@"..\..\something\here.txt", cwd.CreateRelativePath(@"c:\something\here.txt"));
            Assert.Equal(@".\something\here.txt", cwd.CreateRelativePath(@"c:\some\path\something\here.txt"));
            Assert.Equal(@"..\other\something\here.txt", cwd.CreateRelativePath(@"c:\some\other\something\here.txt"));
        }
    }
}
