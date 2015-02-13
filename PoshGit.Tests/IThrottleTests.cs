using PoshGit2;
using System.Threading.Tasks;
using Xunit;

namespace PoshGit.Tests
{
    public class IThrottleTests
    {
        [Fact]
        public async Task MutextThrottle()
        {
            await TestThrottleAsync(new MutexThrottle());
        }

        private async Task TestThrottleAsync(IThrottle throttle)
        {
            var count = 0;
            var task1_ready = new TaskCompletionSource<bool>();
            var task1_wait = new TaskCompletionSource<bool>();

            var task1_task = Task.Run(() =>
            {
                return throttle.TryContinueOrBlock(() =>
                 {
                     Assert.Equal(0, count);
                     task1_ready.SetResult(true);
                     task1_wait.Task.Wait();
                     Assert.Equal(0, count);
                     count++;
                 });
            });

            await task1_ready.Task;

            var task2_ready = new TaskCompletionSource<bool>();
            var task2_wait = new TaskCompletionSource<bool>();

            var task2_task = Task.Run(() =>
            {
                return throttle.TryContinueOrBlock(() =>
                 {
                     Assert.Equal(1, count);
                     task2_ready.SetResult(true);
                     task2_wait.Task.Wait();
                     Assert.Equal(1, count);
                     count++;
                 });
            });

            var task3_ready = new TaskCompletionSource<bool>();
            var task3_wait = new TaskCompletionSource<bool>();

            var task3_task = Task.Run(() =>
            {
                return throttle.TryContinueOrBlock(() =>
                 {
                     Assert.True(false, "Should not enter this block");
                 });
            });

            Assert.False(await task3_task);

            task1_wait.SetResult(true);
            task2_wait.SetResult(true);

            Assert.True(await task1_task);
            Assert.True(await task2_task);

            Assert.Equal(2, count);
        }
    }
}
