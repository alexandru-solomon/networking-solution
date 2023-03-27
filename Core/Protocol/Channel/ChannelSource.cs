
namespace Lithium.Protocol
{
    internal class ChannelSourceSetup:ChannelEndpointSetup
    {
        public ChannelSourceSetup(ChannelConfig channelCfg, ConnectionInfo conInfo, SendDatagramEvent sendDatagramEventHandler): base(channelCfg,conInfo)
        {
            SendDatagramEventHandler = sendDatagramEventHandler;
        }
        public readonly SendDatagramEvent SendDatagramEventHandler;
    }
    public delegate void SendDatagramEvent(byte[] datagram, int offset, int size);

    internal abstract class ChannelSource : ChannelEndpoint
    {
        public ChannelSource(ChannelSourceSetup entryPointSetup) : base(entryPointSetup) 
        {
            SendDatagramEventHandler = entryPointSetup.SendDatagramEventHandler;
        }

        public readonly SendDatagramEvent SendDatagramEventHandler;
    }
}
