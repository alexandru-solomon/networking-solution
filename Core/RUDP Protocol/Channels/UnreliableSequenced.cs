using System;
using System.Collections.Generic;
using System.Text;

namespace Lithium.Net
{
    internal sealed class UnreliableSequencedEmitter : Emitter
    {
        int lastSequence = 0;
        const int SEQUENCE_SIZE = sizeof(int);

        internal override void SendDatagram(byte[] data, int offset, int length)
        {
            lastSequence++;
            //Pass the data to the Transport layer
        }
    }
    internal sealed class UnreliableSequencedReceiver : Receiver
    {
        int lastSequence = 0;
        const int HALF_SEQ_SIZE = int.MaxValue / 2;
        const int SEQUENCE_SIZE = sizeof(int);

        public UnreliableSequencedReceiver(ConnectionInfo connectionInfo) : base(connectionInfo) { }

        internal override void RecieveDatagram(byte[] data, int offset, int size)
        {
            ushort receivedSequence = BitConverter.ToUInt16(data, offset);

            if(receivedSequence > lastSequence)
            {
                //Sequence ID is not residue after wrap
                if (receivedSequence - lastSequence < HALF_SEQ_SIZE)
                    DataReceivedEventHandlers?.Invoke(data, offset + SEQUENCE_SIZE, size);
                    return;
            }
            if (receivedSequence < lastSequence)
            {
                //Sequence ID Wrapped but valid
                if (lastSequence - receivedSequence > HALF_SEQ_SIZE)
                    DataReceivedEventHandlers?.Invoke(data, offset + SEQUENCE_SIZE, size);
                return;
            }
        }
    }

    
}