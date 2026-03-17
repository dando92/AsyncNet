using System;
using System.Threading.Tasks;

namespace AsyncNet.Scheduler
{
    public static class AsyncScheduler
    {
        private static IAsyncScheduler _staticScheduler;

        public static void SetScheduler(IAsyncScheduler scheduler) => _staticScheduler = scheduler;

        public static async Task Post(Func<Task> task)
        {
            if(_staticScheduler == null)
                throw new InvalidOperationException("Scheduler not set");
            
            await _staticScheduler.Post(task);
        }

        public static async Task<T> PostAsync<T>(Func<Task<T>> task)
        {
            if (_staticScheduler == null)
                throw new InvalidOperationException("Scheduler not set");
            
            return await _staticScheduler.Post(task);
        }
    }
}
