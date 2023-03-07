using Lithium;

namespace Lithium
{
    public abstract class NetChannelConfig { }
    public abstract class NetChannel { }

    internal sealed class ReliableChannel : NetChannel
    {
    }
    internal sealed class UnreliableChannel : NetChannel
    {
        void Send(byte[] data)
        {
            CRC32.GetHash(data);
        }
    }
}
