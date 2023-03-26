using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Lithium.Protocol
{
    internal class ActionScheduler : IDisposable
    {
        private readonly int ActionBatch = 10;

        private readonly object queueLock = new object();
        private readonly Stack<Func<bool>> newActionsQueue = new Stack<Func<bool>>();
        private readonly LinkedList<Func<bool>> currentActions = new LinkedList<Func<bool>>();

        private readonly ManualResetEventSlim ActionsAvailable = new ManualResetEventSlim();

        private bool Disposed = false;

        public ActionScheduler()
        {
            Task.Run(SendLoop);
        }

        public void Dispose()
        {
            Disposed = true;
        }

        internal void RequestActions(Func<bool> action)
        {
            lock (queueLock)
            {
                newActionsQueue.Push(action);
                ActionsAvailable.Set(); // wake up the SendLoop thread
            }
        }

        private void SendLoop()
        {
            while (!Disposed)
            {
                //Let each current emitter send a batch of packets
                var node = currentActions.First;
                while (node != null)
                {
                    for (int action = 0; action < ActionBatch; action++)
                    {
                        bool asleep = node.Value.Invoke();

                        if (asleep) break;
                    }
                    currentActions.Remove(node);
                    node = node.Next;
                }
                
                //Add new emitters to the current emitters
                lock (queueLock)
                {
                    //If queue is emptu sleep until a signal is received
                    if (newActionsQueue.Count == 0)
                    {
                        ActionsAvailable.Reset(); // reset the signal to none
                        ActionsAvailable.Wait(); // wait a signal
                    }
                    else continue;

                    //Move the requests from queue to current requests
                    while (newActionsQueue.Count > 0)
                        currentActions.AddFirst(newActionsQueue.Pop());

                    foreach (var action in newActionsQueue)
                    {
                        currentActions.AddFirst(action);
                    }
                    newActionsQueue.Clear();
                    ActionsAvailable.Reset();
                }
            }
        }
    }
}