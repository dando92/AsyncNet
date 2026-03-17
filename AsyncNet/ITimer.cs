using System;

namespace AsyncNet
{

    public interface ITimer
    {
        void ChangeDueTime(int dueTime);
        void Start(Action callback, int dueTime);
        void Stop();
    }
}
