using System;
using System.Collections.Generic;

namespace Lithium.Protocol
{
    internal sealed class UnreliableSequencedChannelConfig : ChannelConfig 
    {
        public UnreliableSequencedChannelConfig() : base(ChannelType.UnreliableSequenced) { }
    }

    internal sealed class UnreliableSequencedEntry : ChannelSource,ISender
    {
        int lastSequence = 0;
        const int SEQUENCE_SIZE = sizeof(int);

        private readonly ActionScheduler scheduler;
        private readonly Queue<byte[]> datagramsBuffer;
        private readonly object bufferLock;
        private readonly SendDatagramEvent sendDatagramEventHandler;

        private bool isSleeping;

        public UnreliableSequencedEntry(ChannelSourceSetup entryPointSetup, SenderSetup emitterSetup) : base(entryPointSetup)
        {
            datagramsBuffer = new Queue<byte[]>();
            bufferLock = new object();
            
            scheduler = emitterSetup.Scheduler;
            
            sendDatagramEventHandler = entryPointSetup.SendDatagramEventHandler;
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

    internal sealed class UnreliableSequencedReceiver : ChannelSink
    {
        int lastSequence = 0;
        const int HALF_SEQ_SIZE = int.MaxValue / 2;
        const int SEQUENCE_SIZE = sizeof(int);

        public UnreliableSequencedReceiver(ChannelSinkSetup exitPointSetup) : base(exitPointSetup) { }

        internal override void RecieveDatagram(byte[] data, int offset, int size)
        {
            ushort receivedSequence = BitConverter.ToUInt16(data, offset);

            if(receivedSequence > lastSequence)
            {
                //Sequence ID is not residue after wrap
                if (receivedSequence - lastSequence < HALF_SEQ_SIZE)
                    OnDataEventHandler?.Invoke(data, offset + SEQUENCE_SIZE, size);
                    return;
            }
            if (receivedSequence < lastSequence)
            {
                //Sequence ID Wrapped but valid
                if (lastSequence - receivedSequence > HALF_SEQ_SIZE)
                    OnDataEventHandler?.Invoke(data, offset + SEQUENCE_SIZE, size);
                return;
            }
        }
    }
}