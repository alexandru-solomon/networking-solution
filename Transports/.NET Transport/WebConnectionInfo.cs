
namespace Lithium.Net
{
    public class WebConnectionInfo:ConnectionInfo
    {
        internal WebConnectionInfo(ushort id) : base(id)
        {
            Id = id;
        }
        public readonly ushort Id;

        public int Ping { get; internal set; }
        public int Uptime { get; internal set; }
    }
}
