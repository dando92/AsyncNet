using System;

namespace AsyncNet.Mock
{
    public class MockTimer : ITimer, ITimeObserver
    {
        private readonly MockTimeLibrary _timeLibrary;
        private Action _callback;
        private int _dueTime;
        private int _nextFireTime;
        private bool _isRunning;
        private bool _disposed;

        public MockTimer(MockTimeLibrary timeLibrary)
        {
            _timeLibrary = timeLibrary;
        }

        public void Start(Action callback, int dueTime)
        {
            _callback = callback;
            _dueTime = dueTime;
            _nextFireTime = _timeLibrary.CurrentTime + dueTime;
            _isRunning = true;
        }

        public void Stop()
        {
            _isRunning = false;
        }

        public void ChangeDueTime(int dueTime)
        {
            _dueTime = dueTime;
            _nextFireTime = _timeLibrary.CurrentTime + dueTime;
        }

        public void OnTimeAdvanced(int newTime)
        {
            if (_isRunning && newTime >= _nextFireTime)
            {
                _nextFireTime = newTime + _dueTime;
                _callback?.Invoke();
            }
        }

        public bool IsExpired() => _disposed;

        public void Dispose()
        {
            _isRunning = false;
            _disposed = true;
        }
    }
}
