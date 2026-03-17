using System;
using System.Threading.Tasks;

namespace AsyncNet.Scheduler
{
    public class ControlledContextAsyncScheduler : IAsyncScheduler
    {
        private SingleThreadContext _context;

        public ControlledContextAsyncScheduler()
        {
            _context = new SingleThreadContext();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task Post(Func<Task> task)
        {
            TaskCompletionSource<object> completion = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            _context.Post(async _ =>
            {
                try
                {
                    // Await the task directly within the posted action
                    await task();
                    completion.SetResult(null);
                }
                catch (OperationCanceledException)
                {
                    completion.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    // This ensures the exception is passed directly to the caller of Post()
                    completion.TrySetException(ex);
                }
            }, null);

            await completion.Task;
        }

        public async Task<T> Post<T>(Func<Task<T>> task)
        {
            TaskCompletionSource<T> completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

            _context.Post(async _ =>
            {
                try
                {
                    // Await the task directly within the posted action
                    var res = await task();
                    completion.SetResult(res);
                }
                catch (OperationCanceledException)
                {
                    completion.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    // This ensures the exception is passed directly to the caller of Post()
                    completion.TrySetException(ex);
                }
            }, null);

            return await completion.Task;
        }
    }
}
