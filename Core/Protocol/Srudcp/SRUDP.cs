using System;

namespace Lithium.Protocol
{
    internal interface ISrudpManager
    {
        public void SendDatagram(byte[] datagram, ConnectionInfo destination);
        public void OnData(byte[] data, int channelId, ConnectionInfo source);
        public void OnDatagramTimeout(ConnectionInfo connection);
    }
    internal class SRUDP
    {
        public readonly ActionScheduler Scheduler = new ActionScheduler();
        public readonly ISrudpManager Manager;

        public SRUDP(ISrudpManager manager, SrudcpConfig protocolConfig)
        {
            Manager = manager;
            Scheduler = new ActionScheduler();
        }

        public void ReceiveDatagram(byte[] datagram)
        {

        }
        public void OnConnection(ConnectionInfo conInfo)
        {

        }
        public void OnDisconnection(ConnectionInfo conInfo)
        {

        }
    }
}