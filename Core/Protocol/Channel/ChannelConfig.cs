namespace Lithium.Protocol
{
    internal enum ChannelType
    {
        Reliable,
        ReliableSequenced,
        ReliableFragmented,
        ReliableFragmentedSequenced,

        Unreliable,
        UnreliableSequenced,
        UnreliableFragmented,
        UnreliableFragmentedSequenced,
    }

    internal abstract class ChannelConfig
    {
        public ChannelConfig(ChannelType quality) 
        {
            Quality = quality;
        }

        public readonly ChannelType Quality;
        public bool UseEncryption = false;
        public int MaxPacketsPerSecond = int.MaxValue;
        public int MasBytesPerSecond = int.MinValue;
        public int MaxDatagramSize = 1024;
    }
}
