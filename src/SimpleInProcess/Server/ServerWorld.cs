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

        private Dictionary<long, NetworkActor> actorMap = new Dictionary<long, NetworkActor>();

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

        public T SpawnNetworkActor<T>(IWirePacketSender? networkRelevantOnlyFor = null, long overrideUniqueId = -1) where T : NetworkActor
        {
            var res = (T?)Activator.CreateInstance(typeof(T), this, -1);
            if (res == null)
            {
                throw new InvalidOperationException();
            }
            res.NetworkRelevantOnlyFor = networkRelevantOnlyFor;
            RegisterNetworkActor(res);
            ReplicateSpawnActor(res);
            return res;
        }

        public void DespawnNetworkActor<T>(T actor) where T : NetworkActor
        {
            UnregisterNetworkActor(actor);
            ReplicateDespawnActor(actor);
        }

        protected void ReplicateExistingSpawns(IWirePacketSender? relevantTarget = null)
        {
            foreach (var actor in actorMap.Values)
            {
                ReplicateSpawnActor(actor, relevantTarget);
            }
        }

        protected void ReplicateSpawnActor<T>(T actor, IWirePacketSender? relevantTarget = null) where T : NetworkActor
        {
            //Console.WriteLine($"ReplicateSpawnActor: {actor}");

            string className = actor.GetReplicationClassName();

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write((int)0);
            writer.Write(Encoding.ASCII.GetByteCount(className));
            writer.Write(Encoding.ASCII.GetBytes(className));
            writer.Write(actor.UniqueId);
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

        protected void ReplicateDespawnActor<T>(T actor, IWirePacketSender? relevantTarget = null) where T : NetworkActor
        {
            //Console.WriteLine($"ReplicateDespawnActor: {actor}");

            string className = actor.GetReplicationClassName();

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write((int)0);
            writer.Write(actor.UniqueId);
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

        public T? GetNetworkActor<T>(long uniqueId) where T : NetworkActor
        {
            if (actorMap.TryGetValue(uniqueId, out NetworkActor? actor))
            {
                return actor as T;
            }

            return null;
        }

        public void RegisterNetworkActor(NetworkActor networkActor)
        {
            if (networkActor.UniqueId == 0)
            {
                throw new InvalidOperationException("UniqueId of NetworkActor is not set up properly.");
            }

            actorMap[networkActor.UniqueId] = networkActor;
        }

        public void UnregisterNetworkActor(NetworkActor networkActor)
        {
            actorMap.Remove(networkActor.UniqueId);
        }

        public void ReplicateValues()
        {
            IWirePacketSender[] receivers = commChannels.ToArray();

            DateTimeOffset now = DateTimeOffset.UtcNow;
            foreach (var item in actorMap)
            {
                var actor = item.Value;
                long id = item.Key;

                var ms = new MemoryStream();
                var writer = new BinaryWriter(ms);
                var shouldRepl = actor.ReplicateValues(writer, false);
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

        public void ReplicateValueDirect(NetworkActor valueOwner, byte[] replicationData)
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
    }

}
