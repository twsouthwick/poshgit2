using Autofac;
using PoshGit2;
using System;
using System.IO;
using Xunit;

namespace PoshGit.Tests
{
    public class AutofacTests
    {
        [Fact]
        public void VerifyAutofacRegistration()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new PoshGitAutofacModule());

            using (var container = builder.Build())
            {
                var factory = container.Resolve<Func<string, IFolderWatcher>>();
                var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                Directory.CreateDirectory(path);

                var watcher = factory(path);

                Assert.IsType<GitFolderWatcher>(watcher);
            }
        }
    }
}
