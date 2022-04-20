using System.Text;
using TicTacToeServer.Networking;
using ULS.Core;

namespace TicTacToeServer.MatchFramework
{
    [UnrealProject(Module = "TicTacToeSample", ProjectFile = @"D:\Projekte\Henning\GitHub\ULS\ULS.Samples\src\TicTacToeSample\Client\TicTacToeSample.uproject", ProjectName = "TicTacToeSample")]
    [UnrealClass(ClassName = "UTicTacToeClientNetworkOwner")]
    public partial class MatchManager : INetworkOwner
    {
        private List<Player> Players = new List<Player>();
        private Dictionary<RemoteClient, Player> PlayerClientLUT = new Dictionary<RemoteClient, Player>();

        private object lockObj = new object();
        private long nextUniqueId = 1;
        private Dictionary<long, NetworkObject> objectMap = new Dictionary<long, NetworkObject>();

        private bool isMatchRunning;

        public MatchManager()
        {
            ResetMatch();
        }

        internal bool TryLogIn(RemoteClient remoteClient, string userName)
        {
            lock (lockObj)
            {
                Console.WriteLine("TryLogin: " + userName + " by " + remoteClient);
                if (Players.Count >= 2)
                {
                    return false;
                }

                // Spawn the Player Actor and set properties
                var player = SpawnNetworkActor<Player>();
                player.RemoteClient = remoteClient;
                PlayerClientLUT.Add(remoteClient, player);
                Players.Add(player);

                ReplicateExistingObjects(remoteClient);

                // Spawn the ClientController for the player and 
                // make it replicate only the corresponding player
                var controller = SpawnNetworkActor<ClientController>(remoteClient);
                controller.Player = player;
                player.PlayerName = userName;
                player.StateValue = Players.IndexOf(player) + 1;

                return true;
            }
        }

        private ClientController? ControllerForPlayer(Player player)
        {
            lock (lockObj)
            {
                var actors = objectMap.Values;
                foreach (var item in actors)
                {
                    var clientContr = item as ClientController;
                    if (clientContr != null)
                    {
                        if (clientContr.Player == player)
                        {
                            return clientContr;
                        }
                    }
                }
            }
            return null;
        }

        internal void PlayerLoggedIn(RemoteClient remoteClient)
        {
            var player = PlayerClientLUT[remoteClient];
            player.IsReady = true;

            Console.WriteLine($"Player {player.PlayerName} logged in");

            if (GetReadyPlayersCount() == 2)
            {
                StartMatch();
            }
        }

        private int GetReadyPlayersCount()
        {
            int count = 0;
            lock (lockObj)
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players[i].IsReady)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        internal void PlayerLoggedOff(RemoteClient remoteClient)
        {
            PlayerClientLUT.TryGetValue(remoteClient, out Player? player);
            if (player == null)
            {
                return;
            }
            Console.WriteLine($"Player {player.PlayerName} logged off");
            PlayerClientLUT.Remove(remoteClient);
            Players.Remove(player);
            DespawnNetworkObject(player);
            var controller = ControllerForPlayer(player);
            if (controller != null)
            {
                DespawnNetworkObject(controller);
            }

            if (Players.Count == 0)
            {
                ResetMatch();
            }
        }

        public long GetNextUniqueId()
        {
            return nextUniqueId++;
        }

        public T? GetNetworkObject<T>(long uniqueId) where T : NetworkObject
        {
            if (objectMap.TryGetValue(uniqueId, out NetworkObject? actor))
            {
                return actor as T;
            }

            return null;
        }

        public T SpawnNetworkObject<T>(IWirePacketSender? networkRelevantOnlyFor = null, long overrideUniqueId = -1) where T : NetworkObject
        {
            lock (lockObj)
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
        }

        public void DespawnNetworkObject<T>(T networkObject) where T : NetworkObject
        {
            lock (lockObj)
            {
                UnregisterNetworkObject(networkObject);
                ReplicateDespawnObject(networkObject);
            }
        }

        public T SpawnNetworkActor<T>(IWirePacketSender? networkRelevantOnlyFor = null, long overrideUniqueId = -1) where T : NetworkActor
        {
            lock (lockObj)
            {
                var res = (T?)Activator.CreateInstance(typeof(T), this, -1);
                if (res == null)
                {
                    throw new InvalidOperationException();
                }
                res.NetworkRelevantOnlyFor = networkRelevantOnlyFor;
                RegisterNetworkObject(res);
                ReplicateSpawnActor(res);
                return res;
            }
        }

        public void DespawnNetworkActor<T>(T networkObject) where T : NetworkActor
        {
            lock (lockObj)
            {
                UnregisterNetworkObject(networkObject);
                ReplicateDespawnActor(networkObject);
            }
        }

        private void ReplicateExistingObjects(IWirePacketSender relevantTarget)
        {
            lock (lockObj)
            {
                var objects = objectMap.Values;

                // Replicate actor spawning first (all actors)
                foreach (var obj in objects)
                {
                    if (obj is NetworkActor)
                    {
                        ReplicateSpawnActor(obj, relevantTarget);
                    }
                    else
                    {
                        ReplicateSpawnObject(obj, relevantTarget);
                    }
                }

                // Then replicate values
                foreach (var obj in objects)
                {
                    ReplicateValuesForObject(obj, DateTimeOffset.MinValue, true, relevantTarget);
                }
            }
        }

        public void ReplicateValueDirect(NetworkObject valueOwner, byte[] replicationData)
        {
            ReplicateValuesDirect(valueOwner, replicationData, null);
        }

        public void ReplicateValuesDirect(NetworkObject valueOwner, byte[] replicationData,
            IWirePacketSender? relevantTarget = null)
        {
            if (valueOwner.NetworkRelevantOnlyFor != null &&
                (relevantTarget != null &&
                 valueOwner.NetworkRelevantOnlyFor != relevantTarget))
            {
                return;
            }

            if (relevantTarget == null &&
                valueOwner.NetworkRelevantOnlyFor != null)
            {
                relevantTarget = valueOwner.NetworkRelevantOnlyFor;
            }

            lock (lockObj)
            {
                IWirePacketSender[] receivers = (IWirePacketSender[])PlayerClientLUT.Keys.ToArray();

                WirePacket wirePacket = new WirePacket(WirePacketType.Replication, replicationData);
                for (int j = 0; j < receivers.Length; j++)
                {
                    receivers[j].SendPacket(wirePacket);
                }
            }
        }

        private void ReplicateValuesForObject(NetworkObject networkObject, DateTimeOffset now, bool forced,
            IWirePacketSender? relevantTarget = null)
        {
            if (networkObject.NetworkRelevantOnlyFor != null &&
                (relevantTarget != null &&
                 networkObject.NetworkRelevantOnlyFor != relevantTarget))
            {
                return;
            }

            if (relevantTarget == null &&
                networkObject.NetworkRelevantOnlyFor != null)
            {
                relevantTarget = networkObject.NetworkRelevantOnlyFor;
            }

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            bool shouldRepl = networkObject.ReplicateValues(writer, forced);
            if (shouldRepl)
            {
                WirePacket wirePacket = new WirePacket(WirePacketType.Replication, ms.ToArray());
                if (relevantTarget == null)
                {
                    IWirePacketSender[] receivers = (IWirePacketSender[])PlayerClientLUT.Keys.ToArray();

                    for (int j = 0; j < receivers.Length; j++)
                    {
                        receivers[j].SendPacket(wirePacket);
                    }
                }
                else
                {
                    relevantTarget.SendPacket(wirePacket);
                }
            }
        }

        public void ReplicateValues()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            long nowTicks = now.Ticks;
            lock (lockObj)
            {
                var actors = objectMap.Values;
                foreach (var actor in actors)
                {
                    long deltaTicks = nowTicks - actor.LastReplicationTimeTicks;
                    if (deltaTicks >= actor.NetUpdateFrequencyTicks)
                    {
                        ReplicateValuesForObject(actor, now, false, null);
                        actor.LastReplicationTimeTicks = nowTicks;
                    }
                }
            }
        }

        public void SendRpc(IWirePacketSender? target, byte[] data)
        {
            WirePacket wirePacket = new WirePacket(WirePacketType.RpcCall, data);

            if (target != null)
            {
                target.SendPacket(wirePacket);
            }
            else
            {
                IWirePacketSender[] receivers = (IWirePacketSender[])PlayerClientLUT.Keys.ToArray();

                for (int j = 0; j < receivers.Length; j++)
                {
                    receivers[j].SendPacket(wirePacket);
                }
            }
        }

        protected void ReplicateSpawnObject<T>(T networkObject, IWirePacketSender? relevantTarget = null) where T : NetworkObject
        {
            ReplicateSpawnObjectInternal(networkObject, WirePacketType.NewObject, relevantTarget);
        }

        protected void ReplicateSpawnActor<T>(T networkObject, IWirePacketSender? relevantTarget = null) where T : NetworkObject
        {
            ReplicateSpawnObjectInternal(networkObject, WirePacketType.SpawnActor, relevantTarget);
        }

        private void ReplicateSpawnObjectInternal<T>(T networkObject, WirePacketType messageType, 
            IWirePacketSender? relevantTarget = null) where T : NetworkObject
        {
            if (networkObject.NetworkRelevantOnlyFor != null &&
                (relevantTarget != null &&
                 networkObject.NetworkRelevantOnlyFor != relevantTarget))
            {
                return;
            }

            if (relevantTarget == null &&
                networkObject.NetworkRelevantOnlyFor != null)
            {
                relevantTarget = networkObject.NetworkRelevantOnlyFor;
            }

            string className = networkObject.GetReplicationClassName();

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write((int)0);
            writer.Write(Encoding.ASCII.GetByteCount(className));
            writer.Write(Encoding.ASCII.GetBytes(className));
            writer.Write(networkObject.UniqueId);
            WirePacket spawnPacket = new WirePacket(messageType, ms.ToArray());

            if (relevantTarget != null)
            {
                relevantTarget.SendPacket(spawnPacket);
            }
            else
            {
                foreach (var item in PlayerClientLUT)
                {
                    item.Key.SendPacket(spawnPacket);
                }
            }
        }

        protected void ReplicateDespawnObject<T>(T networkObject, IWirePacketSender? relevantTarget = null) where T : NetworkObject
        {
            ReplicateDespawnObjectInternal(networkObject, WirePacketType.DestroyObject, relevantTarget);
        }

        protected void ReplicateDespawnActor<T>(T networkObject, IWirePacketSender? relevantTarget = null) where T : NetworkActor
        {
            ReplicateDespawnObjectInternal(networkObject, WirePacketType.DespawnActor, relevantTarget);
        }

        private void ReplicateDespawnObjectInternal<T>(T networkObject, WirePacketType messageType, 
            IWirePacketSender? relevantTarget = null) where T : NetworkObject
        {
            if (networkObject.NetworkRelevantOnlyFor != null &&
                (relevantTarget != null &&
                 networkObject.NetworkRelevantOnlyFor != relevantTarget))
            {
                return;
            }

            if (relevantTarget == null &&
                networkObject.NetworkRelevantOnlyFor != null)
            {
                relevantTarget = networkObject.NetworkRelevantOnlyFor;
            }

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write((int)0);
            writer.Write(networkObject.UniqueId);
            WirePacket spawnPacket = new WirePacket(messageType, ms.ToArray());

            if (relevantTarget != null)
            {
                relevantTarget.SendPacket(spawnPacket);
            }
            else
            {
                foreach (var item in PlayerClientLUT)
                {
                    item.Key.SendPacket(spawnPacket);
                }
            }
        }

        private void RegisterNetworkObject(NetworkObject networkObject)
        {
            if (networkObject.UniqueId == 0)
            {
                throw new InvalidOperationException("UniqueId of NetworkObject is not set up properly.");
            }

            objectMap[networkObject.UniqueId] = networkObject;
        }

        private void UnregisterNetworkObject(NetworkObject networkObject)
        {
            objectMap.Remove(networkObject.UniqueId);
        }        
    }
}
