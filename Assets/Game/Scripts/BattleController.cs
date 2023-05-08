using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Player;
using Game.Scripts.Player.ScriptableObjects;
using Game.Scripts.UI;
using Mirror;
using UnityEngine;

namespace Game.Scripts
{
    public class BattleController : NetworkBehaviour
    {
        [SerializeField] private BattleConfig _battleConfig;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private GameUiWindow _gameUiWindow;

        private GameObject _playerPrefab;
        private System.Random _random = new();
        
        /// <remarks>
        /// Works good if player count lesser or equal to spawn points count.
        /// 
        /// Also player can spawn in another player in case already
        /// spawned player will move in one of the other spawn points.
        /// </remarks>
        private readonly List<Transform> _spawnPointsList = new();
        
        // key - connection id,
        // value - player controller object (can be null)
        private readonly Dictionary<NetworkConnectionToClient, PlayerController> _playerControllers = new();
        
        private bool _endRound;
        private double _roundEndedTime;

        [Server]
        public override void OnStartServer()
        {
            base.OnStartServer();

            _playerPrefab = NetworkManager.singleton.playerPrefab;
            
            _spawnPointsList.AddRange(_spawnPoints);

            NetworkLoop.OnEarlyUpdate += OnNetworkEarlyUpdate;
        }

        [Server]
        public override void OnStopServer()
        {
            _spawnPointsList.Clear();
            
            NetworkLoop.OnEarlyUpdate -= OnNetworkEarlyUpdate;
        }

        [Server]
        private void OnNetworkEarlyUpdate()
        {
            if (!_endRound) return;
            
            var currentTime = NetworkTime.time;
                
            if (currentTime - _roundEndedTime > _battleConfig.RoundRestartSeconds)
            {
                StartNewRound();
            }
        }
        
        [Server]
        public void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            var spawnPoint = _spawnPointsList[_random.Next(_spawnPointsList.Count)];
            _spawnPointsList.Remove(spawnPoint);
            
            var player = PreparePlayerController(conn, spawnPoint);
            
            NetworkServer.AddPlayerForConnection(conn, player.gameObject);
        }
        
        [Server]
        public void OnServerDisconnectPlayer(NetworkConnectionToClient conn)
        {
            _spawnPointsList.Clear();
            _spawnPointsList.AddRange(_spawnPoints);
            
            if (conn.identity != null)
            {
                RemovePlayerController(conn.identity.gameObject.GetComponent<PlayerController>());
            }
        }

        [Server]
        private PlayerController PreparePlayerController(NetworkConnectionToClient conn, Transform spawnPoint)
        {
            var playerObject = Instantiate(_playerPrefab, spawnPoint.position, spawnPoint.rotation);
            var playerController = playerObject.GetComponent<PlayerController>();
            
            playerController.PrepareForSpawn(
                new PlayerMetadata(
                    $"Player {_playerControllers.Count + 1}",
                    _battleConfig.GetRandomPlayerColor(),
                    0)
            );
            
            _playerControllers.Add(conn, playerController);

            playerController.InitializedAndSpawned += OnPlayerSpawned;
            playerController.ServerPlayerMetadataChanged += PlayerMetadataChanged;

            return playerController;
        }

        /// <summary>
        /// Removes player controller from being used (but not destroys it!)
        /// </summary>
        [Server]
        private void RemovePlayerController(PlayerController playerController)
        {
            playerController.InitializedAndSpawned -= OnPlayerSpawned;
            playerController.ServerPlayerMetadataChanged -= PlayerMetadataChanged;
            
            _gameUiWindow.UnregisterPlayer(playerController.netId);
            
            _playerControllers.Remove(playerController.connectionToClient);
        }
        
        [Server]
        private void OnPlayerSpawned(PlayerController playerController)
        {
            _gameUiWindow.UpdatePlayersScoreList(_playerControllers.Values.ToList());
        }
        
        [Server]
        private void PlayerMetadataChanged(PlayerController playerController, PlayerMetadata playerMetadata)
        {
            if (playerMetadata.Score != _battleConfig.MaxScore) return;

            if (!_endRound)
            {
                Debug.Log($"{playerMetadata.Name}, {playerMetadata.Score}, {playerMetadata.TeamColor}");
                EndRound(playerMetadata);
            }
        }
        
        [Server]
        private void EndRound(PlayerMetadata wonPlayerMetadata)
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
            
            Debug.Log($"{wonPlayerMetadata.Name}, {wonPlayerMetadata.Score}, {wonPlayerMetadata.TeamColor}");
            _gameUiWindow.RpcShowGameResultAndRestartTimer(wonPlayerMetadata, _battleConfig.RoundRestartSeconds * 1000);
        }
        
        [Server]
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
                
                var player = PreparePlayerController(connection, spawnPoint);
                NetworkServer.ReplacePlayerForConnection(connection, player.gameObject);
            }
            
            _gameUiWindow.RpcHideGameResult();
        }
        
        private void OnDestroy()
        {
            NetworkLoop.OnEarlyUpdate -= OnNetworkEarlyUpdate;
        }
    }
}