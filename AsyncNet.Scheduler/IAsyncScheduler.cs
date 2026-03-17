using System;
using System.Threading.Tasks;

namespace AsyncNet.Scheduler
{
    public interface IAsyncScheduler : IDisposable
    {
        Task Post(Func<Task> task);
        Task<T> Post<T>(Func<Task<T>> task);
    }

}
