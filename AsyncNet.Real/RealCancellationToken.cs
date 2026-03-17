using System.Threading;
using System.Threading.Tasks;

namespace AsyncNet.Real
{
    public class RealCancellationToken : ICancellationToken
    {
        private readonly CancellationTokenSource _cts;

        public RealCancellationToken(int expireTime)
        {
            if (expireTime > 0)
                _cts = new CancellationTokenSource(expireTime);
            else
                _cts = new CancellationTokenSource();
        }

        public Task Task
        {
            get
            {
                var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

                _cts.Token.Register(() => tcs.TrySetCanceled(_cts.Token));

                return tcs.Task;
            }
        }

        public bool IsCancellationRequested => _cts.IsCancellationRequested;

        public void Cancel()
        {
            _cts.Cancel();
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }

}
