using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Lithium.Transport.Steam")]
[assembly: InternalsVisibleTo("Lithium.Transport.Net")]

namespace Lithium.Network
{
    public enum ServerState { Active, Inactive }

    public abstract class Server
    {
        internal delegate void OnDataReceivedEvent(ushort connectioId, byte[] buffer, ushort offset, ushort length);
        public ServerState State { protected set; get; }
        internal List<ConnectionInfo> Connections = new List<ConnectionInfo>();
        internal Action<ConnectionInfo>? OnConnectionEventHandler;
        internal Action<ConnectionInfo>? OnDisconnectionEventHandler;
        internal Action? OnShutdownEventHandler;

        internal OnDataReceivedEvent? OnDataReceivedEventHandler;


        internal abstract void SendData(ushort connectioId, byte[] buffer, ushort offset, ushort length);
        internal abstract void Disconnect(ushort connectioId);
    }
}