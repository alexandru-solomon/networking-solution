
using System.Collections.Generic;

namespace Lithium.Protocol
{
    internal sealed class UnreliableChannelConfig : ChannelConfig 
    {
        public UnreliableChannelConfig() : base(ChannelType.Unreliable) { }
    }

    internal sealed class UnreliableEmitter : EntryPoint,IEmitter
    {
        private readonly Queue<byte[]> datagramsBuffer;

        public UnreliableEmitter(SRUDP protocol, UnreliableChannelConfig config, ConnectionInfo conInfo, DatagramSendEvent datagramHandler) : base(config, conInfo, datagramHandler)
        {
            ((IEmitter)this).Scheduler = protocol.Scheduler;
            datagramsBuffer = new Queue<byte[]>();
        }

        object IEmitter.BufferLock => throw new System.NotImplementedException();

        bool IEmitter.IsSleeping { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        ActionScheduler IEmitter.Scheduler => throw new System.NotImplementedException();

        void IEmitter.SendData(byte[] data)
        {
            lock (BufferLock)
            {
                datagramsBuffer.Enqueue(data);
                if (IsSleeping)
                {
                    scheduler.RequestActions(SendBufferedDatagram);
                    IsSleeping = false;
                }
            }
        }

        bool IEmitter.SendDatagram()
        {
            lock (BufferLock)
            {
                datagramSender.Invoke(datagramsBuffer.Dequeue(), 0, 0);//NEEDS MODIFICATION

                IsSleeping = datagramsBuffer.Count == 0;
                return IsSleeping;
            }
        }
    }
    
    
    internal sealed class UnreliableReceiver : ExitPoint,IReceiver
    {
        public UnreliableReceiver(UnreliableSequencedChannelConfig config, ConnectionInfo connectionInfo) : base(config, connectionInfo) { }

        void IReceiver.RecieveDatagram(byte[] data, int offset, int size)
        {
            throw new System.NotImplementedException();
        }
    }
}}