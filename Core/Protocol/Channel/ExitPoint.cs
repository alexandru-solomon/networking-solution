
namespace Lithium.Protocol
{
    internal abstract class ExitPoint : AccessPoint
    {
        public delegate void DataEvent(byte[] data, int offset, int length);
        public DataEvent? DataHandler;
        public ExitPoint(ChannelConfig config, ConnectionInfo conInfo) : base(config, conInfo) { }
    }
}
