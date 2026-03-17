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
            CurrentTime += ms;
            
            List<ITimeObserver> expiredObservers = new List<ITimeObserver>();

            lock(_lock)
            {
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
            var ct = new MockedCancellationToken(CurrentTime + ms);
            
            lock (_lock)
            {
                _timeObservers.Add(ct);
            }
                
            return ct;

        }

        public Task Delay(int ms, ICancellationToken token)
        {
            var sleep = new MockedSleep(CurrentTime + ms, token);

            lock (_lock)
            {
                _timeObservers.Add(sleep);
            }

            return sleep.Task;
        }

        public ITimer GetTimer(int dueTime)
        {
            var timer = new MockTimer(this);
            lock (_lock)
            {
                _timeObservers.Add(timer);
            }
            return timer;
        }

        public IEventHandle GetEventHandle(int dueTime)
        {
            var handle = new MockedEventHandle(this);
            lock (_lock)
            {
                _timeObservers.Add(handle);
            }
            return handle;
        }
    }

}
