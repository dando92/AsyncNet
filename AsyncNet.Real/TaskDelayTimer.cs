using System;
using System.Threading.Tasks;

namespace AsyncNet.Real
{
    public class TaskDelayTimer : ITimer
    {
        private ICancellationToken _cts;
        private Action _currentCallback;
        private int _currentDueTime;
        private bool _isRunning;

        public void Start(Action callback, int dueTime)
        {
            if (_isRunning)
                Stop();

            _isRunning = true;
            _currentCallback = callback;
            _currentDueTime = dueTime;
            _cts = Time.GetCancellationToken();

            // Fire and forget the internal loop
            _ = RunTimerLoop(_cts);
        }

        public void Stop()
        {
            _isRunning = false;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        public void ChangeDueTime(int dueTime)
        {
            if (!_isRunning)
                return;

            // Update the time and restart the loop by canceling the current delay
            _currentDueTime = dueTime;

            // Restarting: Stop the current task and start a new one
            Start(_currentCallback, _currentDueTime);
        }

        private async Task RunTimerLoop(ICancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Time.Delay(_currentDueTime, token);

                    // Ensure we don't run the callback if cancellation happened during delay
                    if (!token.IsCancellationRequested)
                    {
                        _currentCallback?.Invoke();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when ChangeDueTime or Stop is called
            }
        }
    }
}
