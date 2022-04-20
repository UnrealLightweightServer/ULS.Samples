using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULS.Core;

namespace SimpleInProcess.Server
{
    public class ServerWorld : INetworkOwner
    {
        private long nextUniqueId = 1;

        private Dictionary<long, NetworkObject> objectMap = new Dictionary<long, NetworkObject>();

        private List<IWirePacketSender> commChannels = new List<IWirePacketSender>();

        protected void Initialize()
        {
            //
        }

        public void AddCommChannel(IWirePacketSender rcv)
        {
            commChannels.Add(rcv);
        }

        public long GetNextUniqueId()
        {
            return nextUniqueId++;
        }

        public T SpawnNetworkObject<T>(IWirePacketSender? networkRelevantOnlyFor = null, long overrideUniqueId = -1) where T : NetworkObject
        {
            var res = (T?)Activator.CreateInstance(typeof(T), this, -1);
            if (res == null)
            {
                throw new InvalidOperationException();
            }
            res.NetworkRelevantOnlyFor = networkRelevantOnlyFor;
            RegisterNetworkObject(res);
            ReplicateSpawnObject(res);
            return res;
        }

        public void DespawnNetworkObject<T>(T networkObject) where T : NetworkObject
        {
            UnregisterNetworkObject(networkObject);
            ReplicateDespawnObject(networkObject);
        }

        protected void ReplicateExistingSpawns(IWirePacketSender? relevantTarget = null)
        {
            foreach (var networkObject in objectMap.Values)
            {
                ReplicateSpawnObject(networkObject, relevantTarget);
            }
        }

        protected void ReplicateSpawnObject<T>(T networkObject, IWirePacketSender? relevantTarget = null) where T : NetworkObject
        {
            string className = networkObject.GetReplicationClassName();

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write((int)0);
            writer.Write(Encoding.ASCII.GetByteCount(className));
            writer.Write(Encoding.ASCII.GetBytes(className));
            writer.Write(networkObject.UniqueId);
            WirePacket spawnPacket = new WirePacket(WirePacketType.SpawnActor, ms.ToArray());

            if (relevantTarget != null)
            {
                relevantTarget.SendPacket(spawnPacket);
            }
            else
            {
                for (int i = 0; i < commChannels.Count; i++)
                {
                    commChannels[i].SendPacket(spawnPacket);
                }
            }
        }

        protected void ReplicateDespawnObject<T>(T networkObject, IWirePacketSender? relevantTarget = null) where T : NetworkObject
        {
            string className = networkObject.GetReplicationClassName();

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write((int)0);
            writer.Write(networkObject.UniqueId);
            WirePacket spawnPacket = new WirePacket(WirePacketType.DespawnActor, ms.ToArray());

            if (relevantTarget != null)
            {
                relevantTarget.SendPacket(spawnPacket);
            }
            else
            {
                for (int i = 0; i < commChannels.Count; i++)
                {
                    commChannels[i].SendPacket(spawnPacket);
                }
            }
        }

        public T? GetNetworkObject<T>(long uniqueId) where T : NetworkObject
        {
            if (objectMap.TryGetValue(uniqueId, out NetworkObject? networkObject))
            {
                return networkObject as T;
            }

            return null;
        }

        public void RegisterNetworkObject(NetworkObject networkObject)
        {
            if (networkObject.UniqueId == 0)
            {
                throw new InvalidOperationException("UniqueId of NetworkObject is not set up properly.");
            }

            objectMap[networkObject.UniqueId] = networkObject;
        }

        public void UnregisterNetworkObject(NetworkObject networkObject)
        {
            objectMap.Remove(networkObject.UniqueId);
        }

        public void ReplicateValues()
        {
            IWirePacketSender[] receivers = commChannels.ToArray();

            DateTimeOffset now = DateTimeOffset.UtcNow;
            foreach (var item in objectMap)
            {
                var networkObject = item.Value;
                long id = item.Key;

                var ms = new MemoryStream();
                var writer = new BinaryWriter(ms);
                var shouldRepl = networkObject.ReplicateValues(writer, false);
                if (shouldRepl)
                {
                    WirePacket wirePacket = new WirePacket(WirePacketType.Replication, ms.ToArray());
                    for (int j = 0; j < receivers.Length; j++)
                    {
                        receivers[j].SendPacket(wirePacket);
                    }
                }
            }
        }

        public void ReplicateValueDirect(NetworkObject valueOwner, byte[] replicationData)
        {
            IWirePacketSender[] receivers = commChannels.ToArray();

            WirePacket wirePacket = new WirePacket(WirePacketType.Replication, replicationData);
            for (int j = 0; j < receivers.Length; j++)
            {
                receivers[j].SendPacket(wirePacket);
            }
        }

        public void SendRpc(IWirePacketSender? target, byte[] data)
        {
            // Not implemented in this sample
        }

        public T SpawnNetworkActor<T>(IWirePacketSender? networkRelevantOnlyFor = null, long overrideUniqueId = -1) where T : NetworkActor
        {
            throw new NotImplementedException();
        }

        public void DespawnNetworkActor<T>(T actor) where T : NetworkObject
        {
            throw new NotImplementedException();
        }
    }

}
