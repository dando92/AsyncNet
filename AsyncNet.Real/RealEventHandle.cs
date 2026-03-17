using System;
using System.Threading.Tasks;

namespace AsyncNet.Real
{
    public class RealEventHandle : IEventHandle
    {
        private readonly object _lock = new object();
        private TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Set()
        {
            lock (_lock)
            {
                _tcs.TrySetResult(true);
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                if (_tcs.Task.IsCompleted)
                    _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }

        public void Wait(int time)
        {
            _tcs.Task.Wait(time);
        }

        public async Task WaitAsync(int time)
        {
            var completed = await Task.WhenAny(_tcs.Task, Task.Delay(time));

            if (completed != _tcs.Task)
                throw new TimeoutException("EventHandle wait timed out");
        }
    }
}
