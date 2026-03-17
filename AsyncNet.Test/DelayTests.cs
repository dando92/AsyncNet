using AsyncNet.Mock;
using AsyncNet.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace AsyncNet.Test
{
    [TestClass]
    public class DelayTests
    {
        private ControlledContextAsyncScheduler _scheduler;
        private MockTimeLibrary _timeLibrary;

        [TestInitialize]
        public void Setup()
        {
            _scheduler = new ControlledContextAsyncScheduler();
            _timeLibrary = new MockTimeLibrary();
            Time.SetTimeLibrary(_timeLibrary);
            AsyncScheduler.SetScheduler(_scheduler);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _timeLibrary.Dispose();
            _scheduler.Dispose();
        }

        [TestMethod]
        public async Task Delay_WithToken_ShouldComplete_WhenTimeAdvances()
        {
            var ct = Time.GetCancellationToken(5000);
            TaskCompletionSource started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var task = AsyncScheduler.Post(async () =>
            {
                started.SetResult();
                await Time.Delay(1000, ct);
            });

            await started.Task;

            _timeLibrary.Advance(1000);

            await task;
            Assert.IsTrue(task.IsCompletedSuccessfully);
        }

        [TestMethod]
        public async Task Delay_WithToken_ShouldCancel_WhenTokenIsManuallyCancelled()
        {
            var ct = Time.GetCancellationToken(5000);
            TaskCompletionSource started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var task = AsyncScheduler.Post(async () =>
            {
                started.SetResult();
                await Time.Delay(2000, ct);
            });

            await started.Task;

            ct.Cancel();

            await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await task);
            Assert.IsTrue(task.IsCanceled);
        }

        [TestMethod]
        public async Task Delay_WithToken_ShouldFault_WhenTokenTimesOutBeforeDelay()
        {
            var ct = Time.GetCancellationToken(500);
            TaskCompletionSource started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var task = AsyncScheduler.Post(async () =>
            {
                started.SetResult();
                await Time.Delay(2000, ct);
            });

            await started.Task;

            _timeLibrary.Advance(500);

            await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await task);
            Assert.IsTrue(task.IsCanceled);
        }
    }
}
