using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncNet.Mock
{
    public class MockedEventHandle : IEventHandle, ITimeObserver
    {
        private readonly MockTimeLibrary _timeLibrary;
        private readonly List<PendingWait> _waiters = new List<PendingWait>();
        private readonly object _lock = new object();
        private bool _isSet;
        private bool _isExpired = false;

        public MockedEventHandle(MockTimeLibrary timeLibrary)
        {
            _timeLibrary = timeLibrary;
        }

        public void Set()
        {
            lock (_lock)
            {
                _isSet = true;
                foreach (var waiter in _waiters)
                    waiter.Tcs.TrySetResult(true);
                _waiters.Clear();
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

        public Task WaitAsync(int time)
        {
            lock (_lock)
            {
                if (_isSet) return Task.CompletedTask;

                var waiter = new PendingWait(_timeLibrary.CurrentTime + time);
                _waiters.Add(waiter);
                return waiter.Tcs.Task;
            }
        }

        public void OnTimeAdvanced(int newTime)
        {
            lock (_lock)
            {
                _waiters.RemoveAll(w =>
                {
                    if (newTime >= w.ExpiryTime)
                    {
                        w.Tcs.TrySetException(new TimeoutException("EventHandle wait timed out"));
                        return true;
                    }
                    return false;
                });
            }
        }

        public bool IsExpired() => _isExpired;

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var waiter in _waiters)
                    waiter.Tcs.TrySetCanceled();

                _waiters.Clear();

                _isExpired = true;
            }
        }

        private class PendingWait
        {
            public TaskCompletionSource<bool> Tcs { get; } = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            public int ExpiryTime { get; }

            public PendingWait(int expiryTime)
            {
                ExpiryTime = expiryTime;
            }
        }
    }
}
