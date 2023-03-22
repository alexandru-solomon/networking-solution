using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Lithium.Transport.Steam")]
[assembly: InternalsVisibleTo("Lithium.Transport.Net")]
[assembly: InternalsVisibleTo("Lithium.Transport.Web")]

namespace Lithium.Network
{
    public abstract class Client
    {
        internal abstract void SendData(byte[] buffer, int offset, int length);
    }
}