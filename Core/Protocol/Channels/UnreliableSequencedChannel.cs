using System;

namespace Lithium.Protocol
{
    internal sealed class UnreliableSequencedChannelConfig : ChannelConfig 
    {
        public UnreliableSequencedChannelConfig() : base(ChannelType.UnreliableSequenced) { }
    }

    internal sealed class UnreliableSequencedEmitter : EntryPoint
    {
        int lastSequence = 0;
        const int SEQUENCE_SIZE = sizeof(int);

        public UnreliableSequencedEmitter(ISrudpManager manager, UnreliableSequencedChannelConfig config, ConnectionInfo connectionInfo) : base(connectionInfo) { }

        internal override void SendDatagram(byte[] data, int offset, int length)
        {
            lastSequence++;
            //Pass the data to the Transport layer
        }
    }
    internal sealed class UnreliableSequencedReceiver : ExitPoint
    {
        int lastSequence = 0;
        const int HALF_SEQ_SIZE = int.MaxValue / 2;
        const int SEQUENCE_SIZE = sizeof(int);

        public UnreliableSequencedReceiver(UnreliableSequencedChannelConfig config, ConnectionInfo connectionInfo) : base(config, connectionInfo) { }

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