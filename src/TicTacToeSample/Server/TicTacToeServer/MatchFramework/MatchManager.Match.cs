using System.Text;
using TicTacToeServer.Networking;
using ULS.Core;
using Timer = System.Timers.Timer;

namespace TicTacToeServer.MatchFramework
{
    public partial class MatchManager : INetworkOwner
    {
        private Timer? replicationTimer;

        private MatchState? matchState = null;

        private Player? activePlayer = null;

        private void StartMatch()
        {
            lock (lockObj)
            {
                if (isMatchRunning == true)
                {
                    return;
                }
                isMatchRunning = true;
            }

            Console.WriteLine("StartMatch called");
            if (Players.Count != 2)
            {
                Console.Error.WriteLine("Can't start match. Player count != 2.");
                return;
            }

            if (matchState == null)
            {
                Console.Error.WriteLine("Can't start match. MatchState not spawned.");
                return;
            }

            replicationTimer = new Timer(1000);
            replicationTimer.Elapsed += ReplicationTimer_Elapsed;
            replicationTimer.AutoReset = true;
            replicationTimer.Enabled = true;

            var ctr0 = ControllerForPlayer(Players[0]);
            var ctr1 = ControllerForPlayer(Players[1]);

            if (ctr0 == null || ctr1 == null)
            {
                Console.Error.WriteLine("Can't start match. Controllers not set up properly");
                return;
            }

            ctr0.OnHandleClick += Ctr_OnHandleClick;
            ctr1.OnHandleClick += Ctr_OnHandleClick;

            matchState.StartMatch();
            SetActivePlayer(Players[0]);
        }

        private void Ctr_OnHandleClick(ClientController controller, int gridX, int gridY)
        {
            Player? player = controller.Player;
            Console.WriteLine($"Player {player} clicked {gridX},{gridY}");
            if (matchState == null)
            {
                return;
            }

            if (activePlayer != player)
            {
                Console.WriteLine($"Cannot handle click from player {player}, because that player is not active.");
                //matchState.IncreaseIncorrectClickCounter();
                return;
            }

            bool success = matchState.PlayerClickedBlock(gridX, gridY, player);
            if (success)
            {
                bool isOver = matchState.CheckWinState(out WinInfo winInfo);
                if (isOver)
                {
                    Console.WriteLine($"Match is over. Winner: {activePlayer} ({winInfo})");
                    matchState.EndMatch(activePlayer);

                    matchState.SetBlockIsMarked(winInfo.X1, winInfo.Y1, true);
                    matchState.SetBlockIsMarked(winInfo.X2, winInfo.Y2, true);
                    matchState.SetBlockIsMarked(winInfo.X3, winInfo.Y3, true);

                    isMatchRunning = false;
                }
                else
                {
                    SetActivePlayer(activePlayer == Players[0] ? Players[1] : Players[0]);
                }
            }
            else
            {
                Console.WriteLine($"Cannot set block at {gridX},{gridY}, because that block is already set.");
                //matchState.IncreaseIncorrectClickCounter();
            }
        }

        private void ReplicationTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            ReplicateValues();
        }

        private void ResetMatch()
        {
            if (matchState != null)
            {
                DespawnNetworkObject(matchState);
                matchState = null;
            }

            isMatchRunning = false;

            matchState = SpawnNetworkActor<MatchState>();
        }

        private void SetActivePlayer(Player player)
        {
            Console.WriteLine("Setting active player to " + player);
            activePlayer = player;
            matchState?.SetActivePlayer(activePlayer);
        }
    }
}
