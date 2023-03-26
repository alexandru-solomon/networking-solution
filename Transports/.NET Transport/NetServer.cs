using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System;
using Lithium.Protocol;

namespace Lithium.Net
{
    public sealed class NetServer : Network.Server,ISrudcpManager, ISrudpManager
    {
        public const ushort DEFAULT_MAX_CONNECTIONS = 8;
        public ushort MaxConnections { private set; get; }
        public ushort Port { private set; get; }
        public string IP { private set; get; }

        private readonly UdpClient udpClient;

        private Dictionary<ushort, ConnectionInfo> connectionsDictionary = new Dictionary<ushort,ConnectionInfo>();
        private List<ConnectionInfo> connections = new List<ConnectionInfo>();

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="SocketException"></exception>
        public NetServer(string ip, ushort port)
        {
            udpClient = new UdpClient(ip, port);
            IP = ip;
            Port = port;
            connections = new List<ConnectionInfo>();
            connectionsDictionary = new Dictionary<ushort, ConnectionInfo>();
        }

        public void SetConnectionsLimit(ushort maxConnections)
        {
            if (connections.Count > maxConnections) 
                DisconnectExcessConnections();

            void DisconnectExcessConnections()
            {
                while(connections.Count > maxConnections) 
                {
                    ConnectionInfo lastestConnectionInfo = connections.Last();
                    ushort lastestConnectionId = lastestConnectionInfo.Id;
                    Disconnect(lastestConnectionId);
                }
            }
            
            MaxConnections = maxConnections;
        }

        

        public void Host()
        {
            State = Network.ServerTransportState
        }
        public void Shutdown()
        {
            State = ServerTransportState.Inactive;
            foreach (var connection in connections) Disconnect(connection.Id);
            OnShutdownEventHandler?.Invoke();
        }
        
        internal override void SendData(ushort connectionId, byte[] buffer, ushort offset, ushort length)
        {
            connectionsDictionary.TryGetValue(connectionId, out var connectionInfo);
 
            if (connectionInfo == null) 
            {
                string message = $"No connection found with the specified connection Id [{connectionId}].";
                throw new InvalidOperationException(message);
            }

            byte[] data = new byte[length];
            Array.Copy(data, offset, buffer, 0, length);
            udpClient.SendAsync(data, length,connectionInfo.IPEndPoint);
        }

        /// <exception cref="InvalidOperationException"></exception>
        internal override void Disconnect(ushort connectionId)
        {
            if(connectionsDictionary.ContainsKey(connectionId) == false)
            {
                string message = "Attempt to disconnect a client that is not connected";
                throw new InvalidOperationException(message);
            }
            ConnectionInfo connectionToRemovoe = connectionsDictionary[connectionId];

            connectionsDictionary.Remove(connectionId);
            connections.Remove(connectionToRemovoe);

            OnDisconnectionEventHandler?.Invoke(connectionToRemovoe);
        }
    }
}