

namespace Lithium.Protocol
{
    internal abstract class EntryPoint : AccessPoint
    {
        public delegate void DatagramSendEvent(byte[] datagram, int offset, int size);
        public EntryPoint(ChannelConfig config, ConnectionInfo conInfo, DatagramSendEvent datagramHandler) : base(config, conInfo) 
        {
            DatagramSender = datagramHandler;
        }

        public readonly DatagramSendEvent DatagramSender;
    }
}
