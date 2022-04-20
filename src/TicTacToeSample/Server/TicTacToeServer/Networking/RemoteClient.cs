using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TicTacToeServer.MatchFramework;
using ULS.Core;

namespace TicTacToeServer.Networking
{
    public class RemoteClient : IWirePacketSender
    {
        protected CommChannel commChannel;

        public event Action? OnDisconnectForced;

        private bool isLoggedIn = false;
        private DateTimeOffset initialConnectionTime = DateTimeOffset.MinValue;

        private MatchManager matchManager;

        public RemoteClient(MatchManager setMatchManager, CommChannel setCommChannel)
        {
            matchManager = setMatchManager;
            isLoggedIn = false;

            initialConnectionTime = DateTimeOffset.UtcNow;

            commChannel = setCommChannel;
            commChannel.PacketReceived += CommChannel_PacketReceived;
        }

        public void OnDisconnected()
        {
            if (commChannel != null)
            {
                commChannel.PacketReceived -= CommChannel_PacketReceived;
            }
        }

        private void CommChannel_PacketReceived(object? sender, WirePacket e)
        {
#if DEBUG
            Console.WriteLine($"Received packet of type {e.PacketType} with {e.Payload.Length} bytes as payload.");
#endif

            switch (e.PacketType)
            {
                case WirePacketType.ConnectionRequest:
                    HandleConnectionRequest(e);
                    break;

                case WirePacketType.RpcCall:
                    HandleRpcCall(e);
                    break;

                default:
                    // TODO: Unhandled
                    break;
            }
        }

        private void HandleRpcCall(WirePacket e)
        {
            if (isLoggedIn == false)
            {
                Console.Error.WriteLine($"Received RpcCall from {this} although client is not logged in.");
                return;
            }

            if (e.PacketType != WirePacketType.RpcCall)
            {
                // TODO: Just ignore or force disconnect?
                return;
            }

            try
            {
                MemoryStream ms = new MemoryStream(e.Payload.ToArray());
                BinaryReader reader = new BinaryReader(ms);
                int flags = reader.ReadInt32();
                long uniqueId = reader.ReadInt64();

                NetworkObject? actor = matchManager.GetNetworkObject<NetworkObject>(uniqueId);
                if (actor == null)
                {
                    Console.WriteLine("Actor for RPC call not found: UniqueId: " + uniqueId);
                    return;
                }

                actor.ProcessRpcMethod(reader);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to handle rpc packet: " + ex);
            }
        }

        private void HandleConnectionRequest(WirePacket e)
        {
            if (e.PacketType != WirePacketType.ConnectionRequest)
            {
                // TODO: Just ignore or force disconnect?
                return;
            }
            
            string userName = e.ReadUnrealString(0);
            string[] splits = userName.Split('-');
            userName = "Player-" + splits[1].Substring(0, 5);

            var loginResult = matchManager.TryLogIn(this, userName);
            Console.WriteLine("loginResult for " + userName + ": " + loginResult);
            if (loginResult == false)
            {
                SendLoginResponsePacket(false);
                ForceDisconnect();
                return;
            }

            isLoggedIn = true;

            SendLoginResponsePacket(true);

            matchManager.PlayerLoggedIn(this);
        }

        public void SendPacket(WirePacket wirepacket)
        {
            commChannel?.EnqueuePacket(wirepacket);
        }

        public void SendPacket(WirePacketType packetType, byte[] data)
        {
            commChannel?.EnqueuePacket(packetType, data);
        }

        public void ForceDisconnect()
        {
            OnDisconnectForced?.Invoke();
        }

        private void SendLoginResponsePacket(bool success)
        {
            byte[] responseData = new byte[1];
            responseData[0] = (byte)(success ? 1 : 0);

            SendPacket(WirePacketType.ConnectionResponse, responseData);
        }

        public override string ToString()
        {
            return $"{commChannel}";
        }
    }
}
