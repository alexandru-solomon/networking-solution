using System.Net;

namespace Lithium.Net
{
    public class NetConnectionInfo:ConnectionInfo
    {
        internal NetConnectionInfo(IPEndPoint ipEndPoint, ushort id) : base(id)
        {
            Port = (ushort)ipEndPoint.Port;
            IP = ipEndPoint.Address.ToString();
        }
        public readonly string IP;
        public readonly ushort Port;
    }
}
