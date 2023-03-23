namespace Lithium.Protocol
{
    internal abstract class ChannelConfig
    {
        public ChannelConfig(ChannelType quality) 
        {
            Quality = quality;
        }
        public readonly ChannelType Quality = ChannelType.Reliable;
        public bool UseEncryption = false;
        public int MaxPacketsPerSecond = int.MaxValue;
        public int MasBytesPerSecond = int.MinValue;
        public int MaxDatagramSize = 1024;
    }
}
