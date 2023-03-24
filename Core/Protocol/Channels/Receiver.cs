﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Lithium.Protocol
{
    internal abstract class Receiver : Endpoint
    {
        public delegate void DataReceivedEvent(byte[] data, int offset, int length);
        public DataReceivedEvent? DataReceivedEventHandlers;
        public Receiver(ChannelConfig config, ConnectionInfo conInfo) : base(config, conInfo) { }

        internal abstract void RecieveDatagram(byte[] data, int offset, int size);
    }
}
