
namespace Lithium
{
    public class ConnectionInfo
    {
        internal ConnectionInfo(ushort id, string ip, ushort port)
        {
            ID = id;
            IP = ip;
            Port = port;
        }
        public readonly ushort ID;
        public readonly string IP;
        public readonly ushort Port;

        public int Ping { get; internal set; }
        public int Uptime { get; internal set; }
    }
}
