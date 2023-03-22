using System.Net.Sockets;
using System.Net;
using System;

namespace Lithium.Net
{
    public sealed class NetClient : Network.Client
    {
        public ushort Port { get; private set; }
        public string IP { get; private set; }

        public ushort RemotePort { get; private set; }
        public string RemoteIP { get; private set; }


        private readonly UdpClient udpClient;

        /// <exception cref="SocketException"></exception>
        public NetClient()
        {
            udpClient = new UdpClient();

            IPEndPoint local = (IPEndPoint)udpClient.Client.LocalEndPoint;
            IP = local.Address.ToString();
            Port = (ushort)local.Port;

            RemotePort = 0;
            RemoteIP = string.Empty;
        }
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="SocketException"></exception>
        public NetClient(string ip, ushort port)
        {
            udpClient = new UdpClient(ip, port);
            IP = ip;
            Port = port;

            RemoteIP = string.Empty;
            RemotePort = 0;
        }

        public void Connect(string ip, ushort port)
        {
            RemotePort = port;
            RemoteIP = ip;
        }
        public void Disconnect()
        {
     
        }
        
        internal override void SendData(byte[] buffer, int offset, int length)
        {
            byte[] data = new byte[length];
            Array.Copy(data, offset, buffer, 0, length);
            udpClient.SendAsync(data, length, connectionInfo.IPEndPoint);
        }
    }
}