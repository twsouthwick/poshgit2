using Microsoft.VisualStudio.TestTools.UnitTesting;
using PoshGit2;
using System.Threading.Tasks;

namespace PoshGit.Tests
{
    [TestClass]
    public class MutexThrottleTests
    {
        [TestMethod]
        public async Task MutextThrottle()
        {
            var queue = new MutexThrottle();

            var task1_ready = new TaskCompletionSource<bool>();
            var task1_wait = new TaskCompletionSource<bool>();

            var task1_task = Task.Run(() =>
            {
                queue.TryContinueOrBlock(() =>
                {
                    task1_ready.SetResult(true);
                    task1_wait.Task.Wait();
                });
            });

            await task1_ready.Task;

            var task2_ready = new TaskCompletionSource<bool>();
            var task2_wait = new TaskCompletionSource<bool>();

            var task2_task = Task.Run(() =>
            {
                queue.TryContinueOrBlock(() =>
                {
                    task2_ready.SetResult(true);
                });
            });

            task1_wait.SetResult(true);

            await task1_task;
            await task2_task;
        }
    }
}
