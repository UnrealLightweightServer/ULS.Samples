using TicTacToeServer.Networking;
using ULS.Core;

namespace TicTacToeServer.MatchFramework
{
    [UnrealActor(UnrealClassName = "/Game/Blueprints/B_Player")]
    public partial class Player : NetworkActor
    {
        public RemoteClient? RemoteClient { get; set; } = null;

        [Replicate]
        private string _playerName = string.Empty;

        [Replicate]
        private int _stateValue = 0;

        internal bool IsReady = false;

        public Player(INetworkOwner setNetworkOwner, long overrideUniqueId) 
            : base(setNetworkOwner, overrideUniqueId)
        {
            IsReady = false;
        }

        public override string ToString()
        {
            return $"{PlayerName} [Remote IP: {RemoteClient}]";
        }
    }
}
