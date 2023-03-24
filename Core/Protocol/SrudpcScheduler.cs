using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Lithium.Protocol
{



    internal class SrudpcScheduler : IDisposable
    {
        private readonly int ActionBatch = 10;

        private readonly object queueLock = new object();
        private readonly Stack<Emitter> emitterQueue = new Stack<Emitter>();
        private readonly LinkedList<Emitter> currentEmitters = new LinkedList<Emitter>();

        private readonly ManualResetEventSlim AvailableData = new ManualResetEventSlim();

        private bool Disposed = false;

        public SrudpcScheduler()
        {
            Task.Run(SendLoop);
        }

        public void Dispose()
        {
            Disposed = true;
        }

        internal void RequestSend(Emitter emitter)
        {
            lock (queueLock)
            {
                emitterQueue.Push(emitter);
                AvailableData.Set(); // wake up the SendLoop thread
            }
        }

        private void SendLoop()
        {
            while (!Disposed)
            {
                var node = currentEmitters.First;
                while (node != null)
                {
                    for (int action = 0; action < ActionBatch; action++)
                    {
                        bool asleep = node.Value.ScheduledSend();

                        if (asleep) break;
                    }
                    currentEmitters.Remove(node);
                    node = node.Next;
                }


                if (currentEmitters.First != null) continue;
               
                lock (queueLock)
                {
                    if (emitterQueue.Count != 0) continue;

                    AvailableData.Reset(); // reset the signal to none
                    AvailableData.Wait(); // wait a signal

                    while (emitterQueue.Count > 0)
                        currentEmitters.AddFirst(emitterQueue.Pop());

                    foreach (Emitter emitter in emitterQueue)
                    {
                        currentEmitters.AddFirst(emitter);
                    }
                    emitterQueue.Clear();
                    AvailableData.Reset();
                }
            }
        }
    }
}