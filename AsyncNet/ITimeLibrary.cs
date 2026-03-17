using System;
using System.Threading.Tasks;

namespace AsyncNet
{
    public interface ITimeLibrary
    {
        Task Delay(int ms);

        Task Delay(int ms, ICancellationToken token);

        ICancellationToken GetCancellationToken(int expireTime);

        ITimer GetTimer(int dueTime);

        IEventHandle GetEventHandle(int dueTime);
    }
}
