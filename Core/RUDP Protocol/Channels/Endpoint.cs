using Lithium.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lithium
{
    internal abstract class Endpoint
    {
        protected Endpoint(ConnectionInfo connectionInfo) 
        {
            ConnectionInfo = connectionInfo;
        }
        public readonly ConnectionInfo ConnectionInfo;
    }
    internal abstract class Receiver : Endpoint
    {
        public delegate void DataReceivedEvent(byte[] data, int offset, int length);
        public DataReceivedEvent? DataReceivedEventHandlers;
        public Receiver(ConnectionInfo connectionInfo) : base(connectionInfo) { }

        internal abstract void RecieveDatagram(byte[] data, int offset, int size);
    }
    internal abstract class Emitter : Endpoint
    {
        internal abstract void SendDatagram(byte[] data, int offset, int length);
        public Emitter(ConnectionInfo connectionInfo) : base(connectionInfo) { }
    }

}
