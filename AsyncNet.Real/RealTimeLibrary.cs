using System.Threading.Tasks;

namespace AsyncNet.Real
{
    public class RealTimeLibrary : ITimeLibrary
    {
        public RealTimeLibrary()
        {
            Time.SetTimeLibrary(this);
        }

        public Task Delay(int ms)
        {
            return Task.Delay(ms);
        }

        public Task Delay(int ms, ICancellationToken token)
        {
            return Task.Delay(ms, (token as RealCancellationToken).Token);
        }

        public ICancellationToken GetCancellationToken(int expireTime)
        {
            return new RealCancellationToken(expireTime);
        }

        public ITimer GetEventHandle(int dueTime)
        {
            throw new System.NotImplementedException();
        }

        public ITimer GetTimer(int dueTime)
        {
            throw new System.NotImplementedException();
        }
    }

}
