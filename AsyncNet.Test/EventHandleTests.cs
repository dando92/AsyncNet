using AsyncNet.Mock;
using AsyncNet.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace AsyncNet.Test
{
    [TestClass]
    public class EventHandleTests
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
        public async Task EventHandle_WaitAsync_ShouldComplete_WhenSetIsCalled()
        {
            var handle = Time.GetEventHandle(0);
            TaskCompletionSource started = new TaskCompletionSource();

            var task = AsyncScheduler.Post(async () =>
            {
                started.SetResult();
                await handle.WaitAsync(5000);
            });

            await started.Task;
            handle.Set();

            await task;
            Assert.IsTrue(task.IsCompletedSuccessfully);
        }

        [TestMethod]
        public async Task EventHandle_WaitAsync_ShouldTimeout_WhenTimeAdvancesPastDueTime()
        {
            var handle = Time.GetEventHandle(0);
            TaskCompletionSource started = new TaskCompletionSource();

            var task = AsyncScheduler.Post(async () =>
            {
                started.SetResult();
                await handle.WaitAsync(1000);
            });

            await started.Task;
            _timeLibrary.Advance(1000);

            await Assert.ThrowsExactlyAsync<TimeoutException>(async () => await task);
        }

        [TestMethod]
        public async Task EventHandle_WaitAsync_ShouldCompleteImmediately_WhenAlreadySet()
        {
            var handle = Time.GetEventHandle(0);
            handle.Set();

            await handle.WaitAsync(1000);
        }

        [TestMethod]
        public async Task EventHandle_Reset_ShouldBlockFutureWaiters()
        {
            var handle = Time.GetEventHandle(0);
            handle.Set();
            handle.Reset();

            TaskCompletionSource started = new TaskCompletionSource();

            var task = AsyncScheduler.Post(async () =>
            {
                started.SetResult();
                await handle.WaitAsync(1000);
            });

            await started.Task;
            Assert.IsFalse(task.IsCompleted);

            handle.Set();
            await task;
            Assert.IsTrue(task.IsCompletedSuccessfully);
        }
    }
}
