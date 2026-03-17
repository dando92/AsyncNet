using System;
using System.Collections.Concurrent;
using System.Threading;

namespace AsyncNet.Scheduler
{
    public class SingleThreadContext : SynchronizationContext, IDisposable
    {
        private readonly BlockingCollection<(SendOrPostCallback callback, object state)> _queue = new BlockingCollection<(SendOrPostCallback callback, object state)>();
        private readonly Thread _thread;

        public SingleThreadContext()
        {
            _thread = new Thread(RunMessageLoop) { IsBackground = true };
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
            try { _queue.Add((d, state)); }
            catch (InvalidOperationException) {}
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            
            if (Thread.CurrentThread.ManagedThreadId == _thread.ManagedThreadId) 
                throw new InvalidOperationException("Cannot dispose SingleThreadContext from its own thread.");

            _thread.Join(); 
            _queue.Dispose();
        }
    }
}
