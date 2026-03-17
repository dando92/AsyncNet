using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncNet.Mock
{
    public class MockedEventHandle : IEventHandle, IDisposable
    {
        private readonly List<TaskCompletionSource<bool>> _setSignals = new List<TaskCompletionSource<bool>>();
        private readonly object _lock = new object();
        private bool _isSet;

        public void Set()
        {
            lock (_lock)
            {
                _isSet = true;
                foreach (var tcs in _setSignals)
                    tcs.TrySetResult(true);
                _setSignals.Clear();
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _isSet = false;
            }
        }

        public void Wait(int time)
        {
            WaitAsync(time).Wait();
        }

        public async Task WaitAsync(int time)
        {
            Task setTask;
            lock (_lock)
            {
                if (_isSet) return;
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                _setSignals.Add(tcs);
                setTask = tcs.Task;
            }

            var delayTask = Time.Delay(time);
            var winner = await Task.WhenAny(setTask, delayTask);

            if (winner != setTask)
                throw new TimeoutException("EventHandle wait timed out");
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var tcs in _setSignals)
                    tcs.TrySetCanceled();
                _setSignals.Clear();
            }
        }
    }
}
