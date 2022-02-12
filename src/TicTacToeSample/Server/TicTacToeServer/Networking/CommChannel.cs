using System.Buffers.Binary;
using System.Net;
using System.Net.WebSockets;
using ULS.Core;

namespace TicTacToeServer.Networking
{
    public class CommChannel
    {
        private object lockObj = new object();
        private AutoResetEvent resetEvent = new AutoResetEvent(false);
        private Queue<WirePacket> packetQueue = new Queue<WirePacket>();

        public event EventHandler<WirePacket>? PacketReceived;

        private IPAddress? _address;
        private int _port;

        public bool IsActive { get; private set; } = false;

        public CommChannel(IPAddress? address, int port)
        {
            _address = address;
            _port = port;
        }

        public void Start()
        {
            IsActive = true;
        }

        public void Stop()
        {
            IsActive = false;
            resetEvent.Set();
        }

        public void EnqueuePacket(WirePacketType packetType, byte[] packetData)
        {
            lock (lockObj)
            {
                packetQueue.Enqueue(new WirePacket(packetType, packetData));
            }
            resetEvent.Set();
        }

        public void EnqueuePacket(WirePacket wirepacket)
        {
            lock (lockObj)
            {
                packetQueue.Enqueue(wirepacket);
            }
            resetEvent.Set();
        }

        public void ProcessReceivedPacket(Memory<byte> data)
        {
            WirePacket packet = new WirePacket(data);
            PacketReceived?.Invoke(this, packet);
        }

        public async Task SendQueuedPackets(WebSocket webSocket)
        {
            resetEvent.WaitOne();

            // May have been set to false, while we were waiting
            if (IsActive == false)
            {
                return;
            }

            //Console.WriteLine($"Sending {packetQueue.Count} packets");
            while (packetQueue.Count > 0)
            {
                WirePacket packet;
                lock (lockObj)
                {
                    packet = packetQueue.Dequeue();
                }
                await webSocket.SendAsync(packet.RawData, WebSocketMessageType.Binary, 
                    true, CancellationToken.None);
            }
        }

        public override string ToString()
        {
            return $"{_address}:{_port}";
        }
    }
}
