using ULS.Core;

namespace TicTacToeServer.MatchFramework
{
    [UnrealActor(UnrealClassName = "/Game/Blueprints/B_MatchState")]
    public partial class MatchState : NetworkActor
    {
        private Grid grid = new Grid();

        /*[Replicate(ReplicationStrategy = ReplicationStrategy.Immediate)]
        private int _incorrectClicks = 0;*/

        public MatchState(INetworkOwner setNetworkOwner, long overrideUniqueId) 
            : base(setNetworkOwner, overrideUniqueId)
        {
            grid.OnBlockOwnershipChanged += Grid_OnBlockOwnershipChanged;
        }

        public bool PlayerClickedBlock(int gridX, int gridY, Player? newOwner)
        {
            return grid.TrySet(newOwner, gridX, gridY);
        }

        /*public void IncreaseIncorrectClickCounter()
        {
            IncorrectClicks++;
        }*/

        private void Grid_OnBlockOwnershipChanged(int gridX, int gridY, Player? newOwner)
        {
            SetBlockOwnership(gridX, gridY, newOwner);
        }

        public bool CheckWinState(out WinInfo winInfo)
        {
            return grid.CheckWinState(out winInfo);
        }

        [RpcCall(CallStrategy = CallStrategy.Reflection)]
        public partial void SetBlockOwnership(int gridX, int gridY, Player? newOwner);

        [RpcCall(CallStrategy = CallStrategy.Reflection)]
        public partial void SetBlockIsMarked(int gridX, int gridY, bool bIsMarked);

        [RpcCall(CallStrategy = CallStrategy.Reflection)]
        public partial void StartMatch();

        [RpcCall(CallStrategy = CallStrategy.Reflection)]
        public partial void EndMatch(Player? winner);

        [RpcCall(CallStrategy = CallStrategy.Reflection)]
        public partial void SetActivePlayer(Player player);
    }
}
