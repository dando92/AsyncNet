using System;

namespace AsyncNet.Mock
{
    public interface ITimeObserver : IDisposable
    {
        void OnTimeAdvanced(int newTime);
        bool IsExpired();
    }

}
