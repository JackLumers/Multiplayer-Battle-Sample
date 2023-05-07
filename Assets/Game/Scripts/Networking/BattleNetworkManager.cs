using Mirror;
using UnityEngine;

namespace Game.Scripts.Networking
{
    public class BattleNetworkManager : NetworkManager
    {
        [SerializeField] private BattleController _battleController;

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            _battleController.Init(playerPrefab);
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            _battleController.OnServerAddPlayer(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            _battleController.OnServerDisconnect(conn);
            
            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }
    }
}