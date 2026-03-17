using System.Threading;
using System.Threading.Tasks;

namespace AsyncNet.Real
{
    public class RealCancellationToken : ICancellationToken
    {
        private readonly CancellationTokenSource _cts;
        private readonly Task _cancellationTask;
        private readonly CancellationTokenRegistration _registration;

        public RealCancellationToken(int expireTime)
        {
            _cts = expireTime > 0
                ? new CancellationTokenSource(expireTime)
                : new CancellationTokenSource();

            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            _registration = _cts.Token.Register(() => tcs.TrySetCanceled(_cts.Token));

            _cancellationTask = tcs.Task;
        }

        public Task Task => _cancellationTask;

        public bool IsCancellationRequested => _cts.IsCancellationRequested;

        public void Cancel() => _cts.Cancel();

        public void Dispose()
        {
            _registration.Dispose();
            _cts.Dispose();
        }
    }

}
