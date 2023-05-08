using Mirror;
using UnityEngine;

namespace Game.Scripts.Networking
{
    public class GameNetworkManager : NetworkManager
    {
        [SerializeField] private BattleController _battleController;
        
        [Server]
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            _battleController.OnServerAddPlayer(conn);
        }

        [Server]
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            _battleController.OnServerDisconnectPlayer(conn);
            
            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }
    }
}