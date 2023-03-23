
namespace Lithium.Protocol
{
    internal sealed class UnreliableChannelConfig : ChannelConfig 
    {
        public UnreliableChannelConfig() : base(ChannelType.Unreliable) { }
    }

    internal sealed class UnreliableEmitter : Emitter
    {
        public UnreliableEmitter(ISrudcpServerManager server, UnreliableSequencedChannelConfig config, ConnectionInfo connectionInfo) : base(config,connectionInfo) { }

        
        internal override void SendDatagram(byte[] data, int offset, int length)
        {
            
        }
    }
    internal sealed class UnreliableReceiver : Receiver
    {
        public UnreliableReceiver(UnreliableSequencedChannelConfig config, ConnectionInfo connectionInfo) : base(config, connectionInfo) { }

        internal override void RecieveDatagram(byte[] data, int offset, int size)
        {
            
        }
    }
}