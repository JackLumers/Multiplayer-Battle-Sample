using System.Collections.Generic;
using Game.Scripts.Player;
using Game.Scripts.UI;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Networking
{
    public class BattleNetworkManager : NetworkManager
    {
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private GameUiWindow _gameUiWindow;

        private List<Transform> _spawnPointsList = new();
        private System.Random _random = new();

        public override void Awake()
        {
            base.Awake();
            
            _spawnPointsList.AddRange(_spawnPoints);
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            var spawnPoint = _spawnPointsList[_random.Next(_spawnPointsList.Count)];
            _spawnPointsList.Remove(spawnPoint);
            
            var player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

            _gameUiWindow.RegisterPlayer(player.GetComponent<PlayerController>());
            
            NetworkServer.AddPlayerForConnection(conn, player);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            _spawnPointsList.Clear();
            _spawnPointsList.AddRange(_spawnPoints);

            _gameUiWindow.UnregisterPlayer(conn.identity.netId);
            
            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }
    }
}