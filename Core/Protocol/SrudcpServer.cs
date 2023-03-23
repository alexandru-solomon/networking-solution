using System;

namespace Lithium.Protocol
{
    interface ISrudcpServerManager
    {
        public void OnConnection(int connectionId);
        public void OnDisconnection(int connectionId);
        public void OnLostConnection(int connectionId);
        public void OnData(byte[] data, int channelId, int connectionId);      
        public void SendDatagram(byte[] data, int connectionId);
    }

    internal class SrudcpServer
    {
        /// <exception cref="ArgumentException"></exception>
        public SrudcpServer(ISrudcpServerManager manager, SrudcpConfig protocolConfig)
        {

        }

        public readonly ISrudcpServerManager Manager;

        public void SendData(ushort connectionID ,byte channelId, byte[] data)
        {
            SendDatagram(data);
        }
        public void ReceiveDatagram(byte[] datagram)
        {

        }        
    }
}
