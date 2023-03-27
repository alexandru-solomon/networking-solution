
using System;
using System.Collections.Generic;

namespace Lithium.Protocol
{
    public sealed class UnreliableChannelConfig : ChannelConfig 
    {
        public UnreliableChannelConfig() : base(ChannelType.Unreliable) { }
    }

    internal sealed class UnreliableSource : ChannelSource , ISender
    {
        private readonly ActionScheduler scheduler;
        private readonly Queue<byte[]> datagramsBuffer;
        private readonly object bufferLock;
        private readonly SendDatagramEvent sendDatagramEventHandler;
        
        private bool isSleeping;

        public UnreliableSource(ChannelSourceSetup channelSourceSetup, SenderSetup senderSetup) : base(channelSourceSetup)
        {
            scheduler = senderSetup.Scheduler;
            datagramsBuffer = new Queue<byte[]>();
            bufferLock = new object();
            sendDatagramEventHandler = channelSourceSetup.SendDatagramEventHandler;
        }

        public void SendData(byte[] data)
        {
            lock (bufferLock)
            {
                datagramsBuffer.Enqueue(data);
                if (isSleeping)
                {
                    scheduler.RequestActions(SendDatagram);
                    isSleeping = false;
                }
            }
        }

        public bool SendDatagram()
        {
            lock (bufferLock)
            {
                sendDatagramEventHandler.Invoke(datagramsBuffer.Dequeue(), 0, 0);//NEEDS MODIFICATION

                isSleeping = datagramsBuffer.Count == 0;
                return isSleeping;
            }
        }
    }
    
    
    internal sealed class UnreliableSink : ChannelSink,IReceiver
    {
        public UnreliableSink(ChannelSinkSetup channelSinkSetup) : base(channelSinkSetup) 
        {
            DataEventHandler = channelSinkSetup.DataEventHandler;
        }

        public void RecieveDatagram(byte[] data, int offset, int size)
        {
            DataEventHandler.Invoke(data, offset, size);
        }
    }
}