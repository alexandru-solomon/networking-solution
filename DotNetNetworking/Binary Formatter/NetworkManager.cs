//Copyright 2023, Alexandru Solomon, All rights reserved.
//Contacts: Alexandru.Solomon.inbox@gmail.com


namespace Networking
{
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Net;
    using System;

    public class NetworkManager
    {
        class Server
        {
            public enum Status { Offline, Online }
            public ushort MaxClientCount { get; set; }
            public ushort ClientCount { get; private set; }
            public string IP { get; private set; }
            public ushort Port { get; private set; }
            public Status State { get; private set; }


            private TcpListener tcpListener;
            private Stack<TcpClient> tcpClients;
            
            public Server()
            {

            }

            public void StartServer(string ip, ushort port, ushort maxClientCount)
            {
                IPAddress.TryParse(ip, out IPAddress address);
                if (address == null) return;
                
                IP = ip;
                Port = port;

                IPEndPoint localEndPoint = new IPEndPoint(address, port);
                tcpClients = new Stack<TcpClient>();
                tcpListener = new TcpListener(localEndPoint);
                tcpListener.Start();
                State = Status.Online;

                ListenForClients();
            }
            public void StopServer()
            {
                foreach(TcpClient client in tcpClients) 
                {
                    client.Close();                    
                }
                tcpClients = null;
                State = Status.Offline;
            }

            void ListenForClients()
            {
                if(ClientCount == MaxClientCount) return;

                AsyncCallback callback = new AsyncCallback(AcceptTcpClient);
                tcpListener.BeginAcceptTcpClient(callback,this);
            }
            void AcceptTcpClient(IAsyncResult result)
            {
                TcpClient client = tcpListener.EndAcceptTcpClient(result);
                tcpClients.Push(client);

                
            }

        }
        class ClientConnection
        {
            TcpClient tcpClient;
            

            public ClientConnection(TcpClient tcpClient)
            {
                this.tcpClient = tcpClient;
            }

            
            
        }
    }
}
