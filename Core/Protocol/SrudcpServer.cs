using System;

namespace Lithium.Protocol
{
    interface ISrudcpManager
    {
        public void OnData(byte[] data, int channelId, int connectionId);      

    }
    interface ISrudcpServerManager
    {
        public void OnConnection(int connectionId);
        public void OnFailedConnection(int connectionId);
        public void OnDisconnection(int connectionId);
        public void OnLostConnection(int connectionId);
        public void SendDatagram(byte[] data, int connectionId);
    }
    internal class SrudcpServer
    {
        public readonly SrudpcScheduler Scheduler = new SrudpcScheduler();
        public readonly ISrudcpServerManager Manager;
 
        /// <exception cref="ArgumentException"></exception>
        public SrudcpServer(ISrudcpServerManager manager, SrudcpConfig protocolConfig)
        {
            Manager = manager;
            Scheduler = new SrudpcScheduler();
        }

        public void ReceiveDatagram(byte[] datagram)
        {

        }        
    }
}
