using System;
using System.Threading.Tasks;

namespace AsyncNet.Real
{
    public class RealTimeLibrary : ITimeLibrary
    {
        public int CurrentTime => Environment.TickCount;

        public Task Delay(int ms)
        {
            return Task.Delay(ms);
        }

        public async Task Delay(int ms, ICancellationToken token)
        {
            var delayTask = Task.Delay(ms);
            var winner = await Task.WhenAny(delayTask, token.Task);

            if (winner != delayTask)
                await token.Task;
        }

        public ICancellationToken GetCancellationToken(int expireTime)
        {
            return new RealCancellationToken(expireTime);
        }

        public IEventHandle GetEventHandle(int dueTime)
        {
            return new RealEventHandle();
        }

        public ITimer GetTimer(int dueTime)
        {
            return new TaskDelayTimer();
        }
    }

}
