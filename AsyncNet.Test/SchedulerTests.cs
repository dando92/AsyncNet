using AsyncNet.Mock;
using AsyncNet.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncNet.Test
{
    [TestClass]
    public class SchedulerTests
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
        public async Task Post_ShouldCompleteSuccessfully()
        {
            var task = AsyncScheduler.Post(async () => await Task.CompletedTask);

            await task;

            Assert.IsTrue(task.IsCompletedSuccessfully);
        }

        [TestMethod]
        public async Task PostAsync_ShouldReturnValue()
        {
            var result = await AsyncScheduler.PostAsync(async () =>
            {
                await Task.CompletedTask;
                return 42;
            });

            Assert.AreEqual(42, result);
        }

        [TestMethod]
        public async Task MultiplePosts_ShouldStartInFifoOrder()
        {
            var events = new List<string>();
            var aStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var bStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var aTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var bTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var taskA = AsyncScheduler.Post(async () =>
            {
                events.Add("A_start");
                aStarted.SetResult();
                await aTcs.Task;
                events.Add("A_end");
            });

            var taskB = AsyncScheduler.Post(async () =>
            {
                events.Add("B_start");
                bStarted.SetResult();
                await bTcs.Task;
                events.Add("B_end");
            });

            await Task.WhenAll(aStarted.Task, bStarted.Task);

            aTcs.SetResult();
            bTcs.SetResult();

            await Task.WhenAll(taskA, taskB);

            CollectionAssert.AreEqual(new[] { "A_start", "B_start", "A_end", "B_end" }, events);
        }

        [TestMethod]
        public async Task MultiplePosts_ShouldInterleave_AtAwaitPoints()
        {
            // A and B both start, then both suspend on Time.Delay.
            // Advancing time fires A first (registered first), then B.
            // Expected event order: A1, B1, A2, B2.
            var events = new List<string>();
            var aStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var bStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var taskA = AsyncScheduler.Post(async () =>
            {
                events.Add("A1");
                aStarted.SetResult();
                await Time.Delay(1000);
                events.Add("A2");
            });

            var taskB = AsyncScheduler.Post(async () =>
            {
                events.Add("B1");
                bStarted.SetResult();
                await Time.Delay(2000);
                events.Add("B2");
            });

            await Task.WhenAll(aStarted.Task, bStarted.Task);

            _timeLibrary.Advance(2000);

            await Task.WhenAll(taskA, taskB);

            CollectionAssert.AreEqual(new[] { "A1", "B1", "A2", "B2" }, events);
        }

        [TestMethod]
        public async Task MultiplePosts_WithDifferentDelays_ShouldCompleteInDelayOrder()
        {
            var events = new List<string>();
            var aStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var bStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var taskA = AsyncScheduler.Post(async () =>
            {
                aStarted.SetResult();
                await Time.Delay(1000);
                events.Add("A");
            });

            var taskB = AsyncScheduler.Post(async () =>
            {
                bStarted.SetResult();
                await Time.Delay(2000);
                events.Add("B");
            });

            await Task.WhenAll(aStarted.Task, bStarted.Task);

            _timeLibrary.Advance(1000);
            await taskA;

            Assert.IsFalse(taskB.IsCompleted, "B should still be waiting after 1000ms");

            _timeLibrary.Advance(1000);
            await taskB;

            CollectionAssert.AreEqual(new[] { "A", "B" }, events);
        }

        [TestMethod]
        public async Task Post_ExceptionInOne_ShouldNotPreventOtherFromCompleting()
        {
            var bStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var taskA = AsyncScheduler.Post(async () =>
            {
                await Task.CompletedTask;
                throw new InvalidOperationException("A failed");
            });

            var taskB = AsyncScheduler.Post(async () =>
            {
                bStarted.SetResult();
                await Task.CompletedTask;
            });

            await bStarted.Task;

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await taskA);
            await taskB;

            Assert.IsTrue(taskA.IsFaulted);
            Assert.IsTrue(taskB.IsCompletedSuccessfully);
        }
    }
}
