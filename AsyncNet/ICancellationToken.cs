using System;
using System.Threading.Tasks;

namespace AsyncNet
{
    public interface ICancellationToken : IDisposable
    {
        bool IsCancellationRequested { get; }
        Task Task { get; }
        void Cancel();
    }
}
