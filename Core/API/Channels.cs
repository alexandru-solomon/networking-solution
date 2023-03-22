using Lithium;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lithium
{
    public enum QOS { Reliable, Unreliable, UnreliableUnordered }
    public abstract class NetChannelConfig
    {
        public string Purpose = "undefined";
        protected const int HEADER_SIZE = 8;

        public int PacketBufferSize = 1024;
        public int MaxPacketSize = 1024 - HEADER_SIZE;

        public int MaxSendRate = 100000;
        public int MaxReceiveRate = 100000;

        public int MaxPacketSendRate = 1024;
        public int MaxPacketReceiveRate = 1024;
    }

    internal abstract class NetChannel
    {
        protected readonly NetChannelConfig Config;
        protected readonly IPEndPoint RemoteEndPoint;
        protected readonly UdpClient Socket;


        protected readonly Queue<Packet> packetBuffer;

        public NetChannel(NetChannelConfig config,IPEndPoint remoteEndPoint, UdpClient socket)
        {
            RemoteEndPoint = remoteEndPoint;
            Config = config;
            Socket = socket;

            packetBuffer = new Packet[config.PacketBufferSize];

            for (int i = 0; i < packetBuffer.Length; i++)
                packetBuffer[i] = new Packet(config.MaxPacketSize);
        }
        protected struct Packet
        {
            public Packet(int maxPacketSize)
            {
                Size = 0;
                Data = new byte[maxPacketSize];
            }
            public byte[] Data;
            public int Size;
            public void Fill(byte[] data, int offset, int size)
            {
                Size = size;
                Buffer.BlockCopy(data, offset, Data, 0, size);
            }
        }
        public abstract void Send(byte[] data, int offset, int size);
        protected abstract void Receive(byte[] buffer);
    }

    public abstract class ReliableChannel : NetChannel
    {
        protected ReliableChannel(NetChannelConfig config, IPEndPoint remoteEndPoint, UdpClient socket) : base(config, remoteEndPoint, socket)
        {
            
        }

        public int PacketRate { get; private set; }
        public int DataRate { get; private set; }

        async Task RateLimiter()
        {
            
            PacketRate = 0;
            DataRate = 0;
            await Task.Delay(1000);
        }

        public override void Send(byte[] data, int offset, int size)
        {
            bool sendLimitReached = (PacketRate + 1 > )
            
        }

        protected override void Receive(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}
