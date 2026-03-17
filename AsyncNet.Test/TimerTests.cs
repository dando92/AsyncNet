using AsyncNet.Mock;
using AsyncNet.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace AsyncNet.Test
{
    [TestClass]
    public class TimerTests
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
        public void Cleanup() => _scheduler.Dispose();

        [TestMethod]
        public void Timer_ShouldFireCallback_WhenTimeAdvancesPastDueTime()
        {
            int callCount = 0;
            var timer = Time.GetTimer(1000);
            timer.Start(() => callCount++, 1000);

            _timeLibrary.Advance(1000);

            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        public void Timer_ShouldNotFireCallback_BeforeDueTime()
        {
            int callCount = 0;
            var timer = Time.GetTimer(1000);
            timer.Start(() => callCount++, 1000);

            _timeLibrary.Advance(500);

            Assert.AreEqual(0, callCount);
        }

        [TestMethod]
        public void Timer_ShouldFireRepeatedly_OnEachInterval()
        {
            int callCount = 0;
            var timer = Time.GetTimer(1000);
            timer.Start(() => callCount++, 1000);

            _timeLibrary.Advance(1000);
            _timeLibrary.Advance(1000);
            _timeLibrary.Advance(1000);

            Assert.AreEqual(3, callCount);
        }

        [TestMethod]
        public void Timer_ShouldNotFireAfterStop()
        {
            int callCount = 0;
            var timer = Time.GetTimer(1000);
            timer.Start(() => callCount++, 1000);

            timer.Stop();
            _timeLibrary.Advance(1000);

            Assert.AreEqual(0, callCount);
        }

        [TestMethod]
        public void Timer_ChangeDueTime_ShouldFireAtNewInterval()
        {
            int callCount = 0;
            var timer = Time.GetTimer(1000);
            timer.Start(() => callCount++, 1000);

            timer.ChangeDueTime(500);
            _timeLibrary.Advance(500);

            Assert.AreEqual(1, callCount);
        }
    }
}
