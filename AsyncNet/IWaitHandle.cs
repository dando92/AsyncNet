namespace AsyncNet
{
    public interface IEventHandle
    {
        void Wait(int time);
        void WaitAsync(int time);
        void Set();
        void Reset();
    }
}
