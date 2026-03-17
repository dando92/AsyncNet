using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncNet.Scheduler
{
    public class ControlledContextAsyncScheduler : IAsyncScheduler
    {
        private readonly SingleThreadContext _context;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public ControlledContextAsyncScheduler()
        {
            _context = new SingleThreadContext();
        }

        public void Dispose()
        {
            _cts.Cancel();
            _context.Dispose();
            _cts.Dispose();
        }

        public Task Post(Func<Task> task) => Post<object>(async () =>
        {
            await task();
            return null;
        });

        public async Task<T> Post<T>(Func<Task<T>> task)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

            CancellationTokenRegistration registration = _cts.Token.Register(() => tcs.TrySetCanceled());

            _context.Post(async _ =>
            {
                try
                {
                    if (_cts.IsCancellationRequested) return;

                    T result = await task();
                    tcs.TrySetResult(result);
                }
                catch (OperationCanceledException)
                {
                    tcs.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
                finally
                {
                    registration.Dispose();
                }
            }, null);

            return await tcs.Task;
        }
    }
}
