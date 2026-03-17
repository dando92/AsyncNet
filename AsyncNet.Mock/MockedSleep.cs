using System.Threading.Tasks;

namespace AsyncNet.Mock
{
    public class MockedSleep : ITimeObserver
    {
        TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();
        private readonly Task _stopTask;

        public Task Task => _tcs.Task;

        public int ExpireTime { get; private set; }

        public MockedSleep(int expireTime, ICancellationToken stopToken = null)
        {
            ExpireTime = expireTime;

            if (stopToken != null)
            {
                _stopTask = stopToken
                    .Task
                    .ContinueWith(restTask =>
                {
                    if (restTask.IsCanceled || restTask.IsFaulted)
                        _tcs.TrySetCanceled();

                }, TaskContinuationOptions.RunContinuationsAsynchronously);
            }
        }

        public void OnTimeAdvanced(int newTime)
        {
            if(newTime >= ExpireTime)
                _tcs.TrySetResult(null);
        }

        public bool IsExpired()
        {
            return _tcs.Task.IsCompleted;
        }

        public void Dispose()
        {
            _tcs.SetCanceled();
            _stopTask.Dispose();
        }
    }
}
