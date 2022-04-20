using ULS.Core;

namespace TicTacToeServer.MatchFramework
{
    [UnrealActor(UnrealClassName = "/Game/Blueprints/B_ClientController")]
    public partial class ClientController : NetworkObject
    {
        [Replicate]
        private Player? _player = null;

        [RpcCall(ParameterNames = new string[] { "controller", "gridX", "gridY" })]
        public event Action<ClientController, int, int> OnHandleClick;

        public ClientController(INetworkOwner setNetworkOwner, long overrideUniqueId) 
            : base(setNetworkOwner, overrideUniqueId)
        {
        }
    }
}
