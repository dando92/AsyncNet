using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncNet.Mock
{
    public class MockTimeLibrary : ITimeLibrary
    {
        public static object _lock = new object();
        public int CurrentTime { get; private set; } = 0;

        List<ITimeObserver> _timeObservers = new List<ITimeObserver>();

        public MockTimeLibrary()
        {
        }

        public void Advance(int ms)
        {
            lock (_lock)
            {
                CurrentTime += ms;

                List<ITimeObserver> expiredObservers = new List<ITimeObserver>();

                foreach (var observer in _timeObservers)
                {
                    observer.OnTimeAdvanced(CurrentTime);

                    if (observer.IsExpired())
                        expiredObservers.Add(observer);
                }

                foreach (var expired in expiredObservers)
                    _timeObservers.Remove(expired);
            }
        }

        public Task Delay(int ms)
        {
            return Delay(ms, null);
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var observer in _timeObservers)
                    observer.Dispose();

                _timeObservers.Clear();
            }
        }

        public ICancellationToken GetCancellationToken(int ms)
        {
            lock (_lock)
            {
                var expireTime = ms <= 0 ? -1 : CurrentTime + ms;
                var ct = new MockedCancellationToken(expireTime);
                _timeObservers.Add(ct);
                return ct;
            }
        }

        public Task Delay(int ms, ICancellationToken token)
        {
            lock (_lock)
            {
                var sleep = new MockedSleep(CurrentTime + ms, token);
                _timeObservers.Add(sleep);
                return sleep.Task;
            }
        }

        public ITimer GetTimer(int dueTime)
        {
            lock (_lock)
            {
                var timer = new MockTimer(this);
                _timeObservers.Add(timer);
                return timer;
            }
        }

        public IEventHandle GetEventHandle(int dueTime)
        {
            lock (_lock)
            {
                var handle = new MockedEventHandle(this);
                _timeObservers.Add(handle);
                return handle;
            }
        }
    }

}
