
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Lithium.Steam")]
[assembly: InternalsVisibleTo("Lithium.Net")]
[assembly: InternalsVisibleTo("Lithium.Web")]

namespace Lithium
{
    public class ConnectionInfo
    {
        internal ConnectionInfo(ushort id)
        {
            Id = id;
        }
        public readonly ushort Id;

        public int Ping { get; internal set; }
        public int Uptime { get; internal set; }
    }
}
