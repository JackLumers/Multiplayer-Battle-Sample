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
            
            var playerObject = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            var playerController = playerObject.GetComponent<PlayerController>();

            playerController.Initialized += OnPlayerControllerInitialized;

            NetworkServer.AddPlayerForConnection(conn, playerObject);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            _spawnPointsList.Clear();
            _spawnPointsList.AddRange(_spawnPoints);

            _gameUiWindow.UnregisterPlayer(conn.identity.netId);
            
            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }
        
        private void OnPlayerControllerInitialized(PlayerController playerController)
        {
            _gameUiWindow.RegisterPlayer(playerController, playerController.PlayerData);
        }
    }
}