

namespace Lithium.Protocol
{
    internal abstract class Emitter : Endpoint
    {
        protected SrudpcScheduler SrudpcScheduler;
        public Emitter( ConnectionInfo conInfo) : base(config,conInfo) 
        {
            SrudpcScheduler = scheduler;
        }
        internal abstract void SendDatagram(byte[] data);
        internal abstract bool ScheduledSend();
    }
}
