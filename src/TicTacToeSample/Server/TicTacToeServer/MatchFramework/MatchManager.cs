using System.Text;
using TicTacToeServer.Networking;
using ULS.Core;

namespace TicTacToeServer.MatchFramework
{
    [UnrealProject(Module = "TicTacToeSample", ProjectFile = "This_needs_to_point_to_the_TicTacToeSample.uproject_file", ProjectName = "TicTacToeSample")]
    [UnrealClass(ClassName = "UTicTacToeClientNetworkOwner")]
    public partial class MatchManager : INetworkOwner
    {
        private List<Player> Players = new List<Player>();
        private Dictionary<RemoteClient, Player> PlayerClientLUT = new Dictionary<RemoteClient, Player>();

        private object lockObj = new object();
        private long nextUniqueId = 1;
        private Dictionary<long, NetworkActor> actorMap = new Dictionary<long, NetworkActor>();

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

                ReplicateExistingActors(remoteClient);

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
                var actors = actorMap.Values;
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
            DespawnNetworkActor(player);
            var controller = ControllerForPlayer(player);
            if (controller != null)
            {
                DespawnNetworkActor(controller);
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

        public T? GetNetworkActor<T>(long uniqueId) where T : NetworkActor
        {
            if (actorMap.TryGetValue(uniqueId, out NetworkActor? actor))
            {
                return actor as T;
            }

            return null;
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
                RegisterNetworkActor(res);
                ReplicateSpawnActor(res);
                return res;
            }
        }

        public void DespawnNetworkActor<T>(T actor) where T : NetworkActor
        {
            lock (lockObj)
            {
                UnregisterNetworkActor(actor);
                ReplicateDespawnActor(actor);
            }
        }

        private void ReplicateExistingActors(IWirePacketSender relevantTarget)
        {
            lock (lockObj)
            {
                var actors = actorMap.Values;

                // Replicate actor spawning first (all actors)
                foreach (var actor in actors)
                {
                    ReplicateSpawnActor(actor, relevantTarget);
                }

                // Then replicate values
                foreach (var actor in actors)
                {
                    ReplicateValuesForActor(actor, DateTimeOffset.MinValue, true, relevantTarget);
                }
            }
        }

        public void ReplicateValueDirect(NetworkActor valueOwner, byte[] replicationData)
        {
            ReplicateValuesDirect(valueOwner, replicationData, null);
        }

        public void ReplicateValuesDirect(NetworkActor valueOwner, byte[] replicationData,
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

        private void ReplicateValuesForActor(NetworkActor networkActor, DateTimeOffset now, bool forced,
            IWirePacketSender? relevantTarget = null)
        {
            if (networkActor.NetworkRelevantOnlyFor != null &&
                (relevantTarget != null &&
                 networkActor.NetworkRelevantOnlyFor != relevantTarget))
            {
                return;
            }

            if (relevantTarget == null &&
                networkActor.NetworkRelevantOnlyFor != null)
            {
                relevantTarget = networkActor.NetworkRelevantOnlyFor;
            }

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            bool shouldRepl = networkActor.ReplicateValues(writer, forced);
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
                var actors = actorMap.Values;
                foreach (var actor in actors)
                {
                    long deltaTicks = nowTicks - actor.LastReplicationTimeTicks;
                    if (deltaTicks >= actor.NetUpdateFrequencyTicks)
                    {
                        ReplicateValuesForActor(actor, now, false, null);
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

        protected void ReplicateSpawnActor<T>(T actor, IWirePacketSender? relevantTarget = null) where T : NetworkActor
        {
            if (actor.NetworkRelevantOnlyFor != null &&
                (relevantTarget != null &&
                 actor.NetworkRelevantOnlyFor != relevantTarget))
            {
                return;
            }

            if (relevantTarget == null &&
                actor.NetworkRelevantOnlyFor != null)
            {
                relevantTarget = actor.NetworkRelevantOnlyFor;
            }

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
                foreach (var item in PlayerClientLUT)
                {
                    item.Key.SendPacket(spawnPacket);
                }
            }
        }

        protected void ReplicateDespawnActor<T>(T actor, IWirePacketSender? relevantTarget = null) where T : NetworkActor
        {
            if (actor.NetworkRelevantOnlyFor != null &&
                (relevantTarget != null &&
                 actor.NetworkRelevantOnlyFor != relevantTarget))
            {
                return;
            }

            if (relevantTarget == null &&
                actor.NetworkRelevantOnlyFor != null)
            {
                relevantTarget = actor.NetworkRelevantOnlyFor;
            }

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
                foreach (var item in PlayerClientLUT)
                {
                    item.Key.SendPacket(spawnPacket);
                }
            }
        }

        private void RegisterNetworkActor(NetworkActor networkActor)
        {
            if (networkActor.UniqueId == 0)
            {
                throw new InvalidOperationException("UniqueId of NetworkActor is not set up properly.");
            }

            actorMap[networkActor.UniqueId] = networkActor;
        }

        private void UnregisterNetworkActor(NetworkActor networkActor)
        {
            actorMap.Remove(networkActor.UniqueId);
        }
    }
}
