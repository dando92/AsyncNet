using System.Collections.Concurrent;
using System.Threading;

namespace AsyncNet.Scheduler
{
    public class SingleThreadContext : SynchronizationContext
    {
        private readonly BlockingCollection<(SendOrPostCallback callback, object state)> _queue = new BlockingCollection<(SendOrPostCallback callback, object state)>();
        private readonly Thread _thread;

        public SingleThreadContext()
        {
            _thread = new Thread(RunMessageLoop);
            _thread.Start();
        }

        private void RunMessageLoop()
        {
            SetSynchronizationContext(this);

            foreach (var item in _queue.GetConsumingEnumerable())
            {
                item.callback(item.state);
            }
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _queue.Add((d, state));
        }

        public void Dispose() => _queue.CompleteAdding();
    }

}
