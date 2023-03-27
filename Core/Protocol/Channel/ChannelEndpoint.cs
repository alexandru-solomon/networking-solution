
namespace Lithium.Protocol
{
    internal interface ISender
    {
        public bool SendDatagram();
        public void SendData(byte[] data);
    }

    internal class SenderSetup
    {
        public SenderSetup(ActionScheduler scheduler) 
        {
            Scheduler = scheduler;
        }   
        public readonly ActionScheduler Scheduler;
    }
    internal class ReceiverSetup
    {
        public ReceiverSetup() 
        {

        }
    }

    internal interface IReceiver
    {
        public void RecieveDatagram(byte[] data, int offset, int size);
    }

    internal abstract class ChannelEndpointSetup
    {
        public ChannelEndpointSetup(ChannelConfig channelConfig, ConnectionInfo conInfo)
        {
            ConnectionInfo = conInfo;
            ChannelConfig = channelConfig;
        }
        public readonly ChannelConfig ChannelConfig;
        public readonly ConnectionInfo ConnectionInfo;
    }

    internal abstract class ChannelEndpoint
    {
        public ChannelEndpoint(ChannelEndpointSetup setup)
        {
            quality = setup.ChannelConfig.Quality;
            MaxPacketsPerSecond = setup.ChannelConfig.MaxPacketsPerSecond;
            MaxBytesPerSecond = setup.ChannelConfig.MasBytesPerSecond;
            ConnectionInfo = setup.ConnectionInfo;
        }

        public readonly ChannelType quality;
        public readonly int MaxPacketsPerSecond;
        public readonly int MaxBytesPerSecond;
        public readonly int MaxDatagramSize;

        public readonly ConnectionInfo ConnectionInfo;
    }
}