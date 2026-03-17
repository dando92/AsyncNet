using AsyncNet.Mock;
using AsyncNet.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace AsyncNet.Test
{
    [TestClass]
    public class WorkflowTests
    {
        private ControlledContextAsyncScheduler _scheduler;
        private MockTimeLibrary _timeLibrary;
        private TaskCompletionSource _tcs;
        private TaskCompletionSource _tcs2;

        [TestInitialize]
        public void Setup()
        {
            _scheduler = new ControlledContextAsyncScheduler();
            _timeLibrary = new MockTimeLibrary();
            _tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _tcs2 = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

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
        public async Task WorkflowB_ShouldComplete_WhenTcsIsSet()
        {
            TaskCompletionSource started = new TaskCompletionSource();

            var task = AsyncScheduler.Post(async () =>
            {
                started.SetResult();
                await _tcs2.Task;
            });

            await started.Task;
            _tcs2.SetResult();

            await task;
            Assert.IsTrue(task.IsCompletedSuccessfully);
        }

        [TestMethod]
        public async Task WorkflowA_ShouldComplete_OnlyAfterTimeAdvancesAndTcsIsSet()
        {
            var ct = Time.GetCancellationToken(5000);
            TaskCompletionSource started = new TaskCompletionSource();

            var task = AsyncScheduler.Post(async () =>
            {
                started.SetResult();

                await Time.Delay(2000);
                await Task.WhenAny(_tcs.Task, ct.Task);
            });

            await started.Task;

            _tcs.SetResult();
            Assert.IsFalse(task.IsCompleted, "Task should still be waiting on Time.Delay(2000)");

            _timeLibrary.Advance(2000);

            await task;
            Assert.IsTrue(task.IsCompletedSuccessfully);
        }

        [TestMethod]
        public async Task WorkflowC_ShouldFault_WhenTokenReachesTimeout()
        {
            var ct2 = Time.GetCancellationToken(100);
            TaskCompletionSource started = new TaskCompletionSource();
            var task = AsyncScheduler.Post(async () => { started.SetResult(); await ct2.Task; });

            await started.Task;

            _timeLibrary.Advance(200);

            await Assert.ThrowsExactlyAsync<TimeoutException>(async () => await task);
            Assert.IsTrue(task.IsFaulted);
        }

        [TestMethod]
        public async Task WorkflowD_ShouldCancel_WhenManualCancelIsCalled()
        {
            var ct3 = Time.GetCancellationToken(3000);
            TaskCompletionSource started = new TaskCompletionSource();
            var task = AsyncScheduler.Post(async () => { started.SetResult(); await ct3.Task; });

            await started.Task;

            ct3.Cancel();

            await Assert.ThrowsExactlyAsync<TaskCanceledException>(async () => await task);
            Assert.IsTrue(task.IsCanceled);
        }
    }
}
