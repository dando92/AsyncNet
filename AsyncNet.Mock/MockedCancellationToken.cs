using System;
using System.Threading.Tasks;

namespace AsyncNet.Mock
{
    public class MockedCancellationToken : ITimeObserver, ICancellationToken
    {
        private readonly TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly int _expireTime;
        private bool _canceled;

        public Task Task => _tcs.Task;

        public bool IsCancellationRequested => _canceled;

        public MockedCancellationToken(int expireTime = -1)
        {
            _expireTime = expireTime;
        }

        public void OnTimeAdvanced(int newTime)
        {
            if (_expireTime > 0 && newTime >= _expireTime)
            {
                _tcs.TrySetException(new TimeoutException($"Timeout expired"));
            }
        }

        public void Cancel()
        {
            _canceled = true;
            _tcs.TrySetCanceled();
        }

        public bool IsExpired() => _tcs.Task.IsCanceled;

        public void Dispose()
        {
            _tcs.TrySetException(new ObjectDisposedException("MockedCancellationToken"));
        }
    }
}
