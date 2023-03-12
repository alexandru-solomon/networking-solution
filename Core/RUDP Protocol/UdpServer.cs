using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using Lithium.Transport.Net;

namespace Lithium.Net
{
    internal enum DatagramType { CONNECT, CONFIG, DATA, ACK, DISCONNECT}
    internal class UdpServer
    {
        UdpClient? udpClient;
        List<ConnectionInfo> connections = new List<ConnectionInfo>();
        IdentifierPool idPool = new IdentifierPool();


        public void Start(string ip, ushort port)
        {
            udpClient = new UdpClient(ip, port);
            connections = new List<ConnectionInfo>();
            udpClient.BeginReceive(new AsyncCallback(OnReceivedDatagram), null);
        }

        private void OnReceivedDatagram(IAsyncResult asyncResult)
        {
            IPEndPoint? iPEndPoint = null;

            byte[] data = udpClient.EndReceive(asyncResult ,ref iPEndPoint);
            if (data == null) return;  
            if (data.Length == 0) return; //Packet is empty
            byte header = data[0];

            DatagramType type = (DatagramType)((header & 0b11100000) >> 5);
            //int channelId = (header & 0b00011111);

            if(type == DatagramType.CONNECT)
            {
                if (connections.Any(connection => connection.IPEndPoint == iPEndPoint)) Malfunction();

                connections.Add(new ConnectionInfo(iPEndPoint, idPool.Rent()));


            }



        }

        private void Malfunction()
        {

        }

        public Action? OnConnection;
        public Action? OnConnectionAttempt;
    }
}
