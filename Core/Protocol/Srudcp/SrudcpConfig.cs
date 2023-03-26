using System.Collections.Generic;

namespace Lithium.Protocol
{
    internal class SrudcpConfig
    {
        public IEnumerable<ChannelConfig>? ServerChannels;
        public IEnumerable<ChannelConfig>? ClientChannels;
    }
}
