using SimpleInProcess.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULS.Core;

namespace SimpleInProcess.Client
{
    public class ClientNetworkOwner : INetworkOwner
    {
        protected Dictionary<long, NetworkObject> actorMap = new Dictionary<long, NetworkObject>();

        public long GetNextUniqueId()
        {
            return -1;
        }

        public T SpawnNetworkObject<T>(IWirePacketSender? networkRelevantOnlyFor = null, long overrideUniqueId = -1) where T : NetworkObject
        {
            var res = (T?)Activator.CreateInstance(typeof(T), this, overrideUniqueId);
            if (res == null)
            {
                throw new InvalidOperationException();
            }
            RegisterNetworkActor(res);
            return res;
        }

        public void DespawnNetworkObject<T>(T actor) where T : NetworkObject
        {
            UnregisterNetworkActor(actor);
        }

        public T? GetNetworkObject<T>(long uniqueId) where T : NetworkObject
        {
            if (actorMap.TryGetValue(uniqueId, out NetworkObject? actor))
            {
                return actor as T;
            }

            return null;
        }

        public void RegisterNetworkActor(NetworkObject networkActor)
        {
            if (networkActor.UniqueId == 0)
            {
                throw new InvalidOperationException("UniqueId of NetworkActor is not set up properly.");
            }

            //Console.WriteLine($"Registering actor with id {networkActor.UniqueId}");
            actorMap[networkActor.UniqueId] = networkActor;
        }

        public void UnregisterNetworkActor(NetworkObject networkActor)
        {
            //Console.WriteLine($"Unregistering actor with id {networkActor.UniqueId}");
            actorMap.Remove(networkActor.UniqueId);
        }

        public void ReplicateValues()
        {
            // Nothing to do on client
        }

        public void ReplicateValueDirect(NetworkObject valueOwner, byte[] replicationData)
        {
            // Nothing to do on client
        }

        public void SendRpc(IWirePacketSender? target, byte[] data)
        {
            //
        }
    }

    public class ClientWorld : ClientNetworkOwner, IWirePacketSender
    {
        public void SendPacket(WirePacket packet)
        {
            //Console.WriteLine("Handle packet: " + packet.PacketType);

            switch (packet.PacketType)
            {
                case WirePacketType.Replication:
                    HandleReplication(packet);
                    break;

                case WirePacketType.SpawnActor:
                    HandleSpawnActor(packet);
                    break;

                case WirePacketType.DespawnActor:
                    HandleDespawnActor(packet);
                    break;
            }
        }

        private void HandleReplication(WirePacket packet)
        {
            MemoryStream ms = new MemoryStream(packet.Payload.ToArray());
            BinaryReader reader = new BinaryReader(ms);

            int flags = reader.ReadInt32();     // Flags
            long id = reader.ReadInt64();

            var actor = GetNetworkObject<NetworkObject>(id);
            if (actor != null)
            {
                actor.ApplyReplicatedValues(reader);
            }
            else
            {
                Console.Error.WriteLine("Cannot handle replication for actor with id: " + id + " => Actor not known");
            }
        }

        private void HandleSpawnActor(WirePacket packet)
        {
            MemoryStream ms = new MemoryStream(packet.Payload.ToArray());
            BinaryReader reader = new BinaryReader(ms);

            reader.ReadInt32();     // Flags
            string str = Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadInt32()));
            long id = reader.ReadInt64();

            if (actorMap.ContainsKey(id))
            {
                Console.Error.WriteLine("Cannot spawn actor with id: " + id + " => Actor already exists");
            }
            else
            {
                //Console.WriteLine($"Spawn actor of type {str} with id {id}");

                switch (str)
                {
                    case "Actor":
                        SpawnNetworkObject<Actor>(null, id);
                        break;

                    case "SubActor":
                        SpawnNetworkObject<SubActor>(null, id);
                        break;
                }
            }
        }

        private void HandleDespawnActor(WirePacket packet)
        {
            MemoryStream ms = new MemoryStream(packet.Payload.ToArray());
            BinaryReader reader = new BinaryReader(ms);
            reader.ReadInt32();     // Flags
            long id = reader.ReadInt64();

            var actor = GetNetworkObject<NetworkObject>(id);
            if (actor != null)
            {
                DespawnNetworkObject(actor);
            }
            else
            {
                Console.Error.WriteLine("Cannot despawn actor with id: " + id + " => Actor not known");
            }
        }
    }
}
