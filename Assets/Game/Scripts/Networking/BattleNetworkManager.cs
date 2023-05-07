using System.Collections.Generic;
using Game.Scripts.Player;
using Game.Scripts.Player.ScriptableObjects;
using Game.Scripts.UI;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Networking
{
    public class BattleNetworkManager : NetworkManager
    {
        [SerializeField] private BattleConfig _battleConfig;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private GameUiWindow _gameUiWindow;

        private System.Random _random = new();

        private List<Transform> _spawnPointsList = new();
        
        // key - connection id,
        // value - player controller object (can be null)
        private readonly Dictionary<NetworkConnectionToClient, PlayerController> _playerControllers = new();
        
        private bool _endRound;
        private double _roundEndedTime;
        
        public override void Awake()
        {
            base.Awake();
            
            _spawnPointsList.AddRange(_spawnPoints);
            
            NetworkLoop.OnEarlyUpdate += OnNetworkEarlyUpdate;
        }

        private void OnNetworkEarlyUpdate()
        {
            if (!_endRound) return;
            
            var currentTime = NetworkTime.time;
                
            if (currentTime - _roundEndedTime > _battleConfig.RoundRestartSeconds)
            {
                StartNewRound();
            }
        }
        
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            var spawnPoint = _spawnPointsList[_random.Next(_spawnPointsList.Count)];
            _spawnPointsList.Remove(spawnPoint);
            
            var player = SpawnPlayerController(conn, spawnPoint);
            
            NetworkServer.AddPlayerForConnection(conn, player.gameObject);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (_playerControllers.ContainsKey(conn))
            {
                RemovePlayerController(conn.identity.GetComponent<PlayerController>());
            }
            
            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }

        private PlayerController SpawnPlayerController(NetworkConnectionToClient conn, Transform spawnPoint)
        {
            var playerObject = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            var playerController = playerObject.GetComponent<PlayerController>();
            
            playerController.Initialized += OnPlayerControllerInitialized;
            playerController.MetaDataChanged += OnPlayerMetadataChanged;

            _playerControllers.Add(conn, playerController);

            return playerController;
        }

        /// <summary>
        /// Removes player controller from being used (but not destroys it!)
        /// </summary>
        private void RemovePlayerController(PlayerController playerController)
        {
            playerController.Initialized -= OnPlayerControllerInitialized;
            playerController.MetaDataChanged -= OnPlayerMetadataChanged;
            
            _gameUiWindow.UnregisterPlayer(playerController.connectionToClient.connectionId);
            
            _playerControllers.Remove(playerController.connectionToClient);
        }

        private void OnPlayerControllerInitialized(PlayerController playerController)
        {
            _gameUiWindow.RegisterPlayer(playerController, playerController.MetaPlayerData);
        }
        
        private void OnPlayerMetadataChanged(PlayerController playerController, MetaPlayerData metaPlayerData)
        {
            if (metaPlayerData.Score != _battleConfig.MaxScore) return;

            if (!_endRound)
            {
                EndRound(metaPlayerData);
            }
        }
        
        private void EndRound(MetaPlayerData wonPlayerMetadata)
        {
            _endRound = true;
            _roundEndedTime = NetworkTime.time;
            
            foreach (var connection in NetworkServer.connections.Values)
            {
                if (_playerControllers.TryGetValue(connection, out var playerController))
                {
                    RemovePlayerController(playerController);
                    NetworkServer.Destroy(playerController.gameObject);
                }
            }
            
            _gameUiWindow.ShowGameResultAndRestartTimer(wonPlayerMetadata, _battleConfig.RoundRestartSeconds * 1000);
        }
        
        private void StartNewRound()
        {
            _endRound = false;

            _spawnPointsList.Clear();
            _spawnPointsList.AddRange(_spawnPoints);
            
            foreach (var connection in NetworkServer.connections.Values)
            {
                if (_playerControllers.TryGetValue(connection, out var playerController))
                {
                    RemovePlayerController(playerController);
                    NetworkServer.Destroy(playerController.gameObject);
                }

                var spawnPoint = _spawnPointsList[_random.Next(_spawnPointsList.Count)];
                _spawnPointsList.Remove(spawnPoint);
                
                var player = SpawnPlayerController(connection, spawnPoint);
                NetworkServer.ReplacePlayerForConnection(connection, player.gameObject);
            }
            
            _gameUiWindow.HideGameResult();
        }
    }
}