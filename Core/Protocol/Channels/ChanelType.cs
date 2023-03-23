using System;
using System.Collections.Generic;
using System.Text;

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
}
