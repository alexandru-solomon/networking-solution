
namespace Lithium.Protocol
{
    internal interface IEmitter
    {
        public object BufferLock { get; }
        public bool IsSleeping { get; protected set; }
        public ActionScheduler Scheduler { get; }

        protected abstract bool SendDatagram();
        public void SendData(byte[] data);
    }
    internal interface IReceiver
    {
        internal abstract void RecieveDatagram(byte[] data, int offset, int size);
    }

    internal abstract class AccessPoint
    {
        public AccessPoint(ChannelConfig config, ConnectionInfo connectionInfo)
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