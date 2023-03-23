
namespace Lithium.Protocol
{
    internal abstract class Endpoint
    {
        public Endpoint(ChannelConfig config, ConnectionInfo connectionInfo)
        {
            quality = config.Quality;
            MaxPacketsPerSecond = config.MaxPacketsPerSecond;
            MaxBytesPerSecond = config.MasBytesPerSecond;
            ConnectionInfo = connectionInfo;
        }

        public readonly ChannelType quality;
        public readonly int MaxPacketsPerSecond;
        public readonly int MaxBytesPerSecond;
        public readonly int MaxDatagramSize;

        public readonly ConnectionInfo ConnectionInfo;
    }
}
