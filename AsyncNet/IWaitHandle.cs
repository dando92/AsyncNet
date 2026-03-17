using System.Threading.Tasks;

namespace AsyncNet
{
    public interface IEventHandle
    {
        void Wait(int time);
        Task WaitAsync(int time);
        void Set();
        void Reset();
    }
}
