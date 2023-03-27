
namespace Lithium.Protocol
{
    internal class ChannelSinkSetup : ChannelEndpointSetup
    {
        public ChannelSinkSetup(ChannelConfig channelConfig, ConnectionInfo conInfo, DataEvent dataEventHandler) : base(channelConfig,conInfo)
        {
            DataEventHandler = dataEventHandler;
        }
        public DataEvent DataEventHandler;
    }
    public delegate void DataEvent(byte[] data, int offset, int length);
    internal abstract class ChannelSink : ChannelEndpoint
    {
        public ChannelSink(ChannelSinkSetup exitPointSetup) : base(exitPointSetup) 
        {
            DataEventHandler = exitPointSetup.DataEventHandler;
        }
        public DataEvent DataEventHandler;
    }
}
