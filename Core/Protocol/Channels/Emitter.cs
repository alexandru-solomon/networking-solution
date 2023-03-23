
namespace Lithium.Protocol
{
    internal abstract class Emitter : Endpoint
    {
        internal abstract void SendDatagram(byte[] data, int offset, int length);
        public Emitter(ChannelConfig config, ConnectionInfo connectionInfo) : base(config,connectionInfo) { }
    }
}
