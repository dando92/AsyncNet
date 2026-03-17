using System.Threading.Tasks;

namespace AsyncNet
{
    public static class Time
    {
        static ITimeLibrary _timeLibrary;

        public static void SetTimeLibrary(ITimeLibrary library)
        {
            _timeLibrary = library;
        }

        public static async Task Delay(int milliseconds)
        {
            await _timeLibrary.Delay(milliseconds);
        }

        public static async Task Delay(int milliseconds, ICancellationToken stopToken)
        {
            await _timeLibrary.Delay(milliseconds, stopToken);
        }

        public static ICancellationToken GetCancellationToken(int expireTime = -1)
        {
            return _timeLibrary.GetCancellationToken(expireTime);
        }

        public static ITimer GetTimer(int dueTime)
        {
            return _timeLibrary.GetTimer(dueTime);
        }

        public static IEventHandle GetEventHandle(int dueTime)
        {
            return _timeLibrary.GetEventHandle(dueTime);
        }
    }
}
