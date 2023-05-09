using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Player;
using Game.Scripts.Player.ScriptableObjects;
using Mirror;
using UnityEngine;

namespace Game.Scripts.UI
{
    [RequireComponent(typeof(Canvas))]
    public class GameUiWindow : NetworkBehaviour
    {
        [SerializeField] private Transform _scoreElementsLayout;
        [SerializeField] private RoundResultElement _roundResultElement;
        [SerializeField] private PlayerScoreElement _playerScoreElementPrefab;
        
        // Key = NetId
        private readonly Dictionary<uint, PlayerScoreElement> _enabledClientPlayerScoreElements = new();
        private readonly Dictionary<uint, PlayerController> _registeredPlayerControllers = new();
        
        // Can be taken from Pooling System + Addressable Assets Async Instantiation,
        // but for sake of simplicity will do.
        private readonly Queue<PlayerScoreElement> _disabledPlayerScoreElements = new();

        [Server]
        public void UpdatePlayersScoreList(List<PlayerController> players)
        {
            Debug.Log("UpdatePlayersScoreList!");
            
            RpcUpdatePlayersList(players);
        }

        [Server]
        public void UnregisterPlayer(uint playerNetId)
        {
            RpcRemovePlayerScoreElement(playerNetId);
        }
        
        [ClientRpc]
        public void RpcShowGameResultAndRestartTimer(PlayerMetadata winnerMetadata, int restartMillis)
        {
            Debug.Log($"{winnerMetadata.Name}, {winnerMetadata.Score}, {winnerMetadata.TeamColor}");

            _roundResultElement.SetWinnerData(winnerMetadata);
            _roundResultElement.StartVisualTimer(restartMillis).Forget();
            
            _roundResultElement.gameObject.SetActive(true);
        }

        [ClientRpc]
        public void RpcHideGameResult()
        {
            _roundResultElement.gameObject.SetActive(false);
        }
        
        [ClientRpc]
        private void RpcUpdatePlayersList(List<PlayerController> players)
        {
            foreach (var player in players)
            {
                // Update already enabled if such exist
                if (_enabledClientPlayerScoreElements.ContainsKey(player.netId))
                {
                    _enabledClientPlayerScoreElements[player.netId].SetData(player.PlayerMetadata);
                }
                // Create new score element and populate it's data
                else
                {
                    // Get from disabled if such exist
                    if (!_disabledPlayerScoreElements.TryDequeue(out var playerScoreElement))
                    {
                        playerScoreElement = Instantiate(_playerScoreElementPrefab, _scoreElementsLayout);
                    }

                    _registeredPlayerControllers.Add(player.netId, player);
                    _enabledClientPlayerScoreElements.Add(player.netId, playerScoreElement);

                    playerScoreElement.SetData(player.PlayerMetadata);

                    player.ClientPlayerMetadataChanged += PlayerMetadataChanged;

                    playerScoreElement.gameObject.SetActive(true);
                }
            }
        }
        
        [ClientRpc]
        private void RpcRemovePlayerScoreElement(uint playerNetId)
        {
            RemovePlayerScoreElement(playerNetId);
        }
        
        [Client]
        private void PlayerMetadataChanged(PlayerController playerController, PlayerMetadata newMetadata)
        {
            if (_enabledClientPlayerScoreElements.TryGetValue(playerController.netId,
                    out var playerScoreElement))
            {
                playerScoreElement.SetData(newMetadata);
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            
            _roundResultElement.gameObject.SetActive(false);
            
            // Bugfix: prevents duplication of player score elements if client disconnects and connects again.
            // Could be more optimized
            foreach (var registeredPlayerController in _registeredPlayerControllers.Keys.ToList())
            {
                RemovePlayerScoreElement(registeredPlayerController);
            }
        }
        
        private void RemovePlayerScoreElement(uint playerNetId)
        {
            if (!_registeredPlayerControllers.ContainsKey(playerNetId)) 
                return;
            
            var scoreElement = _enabledClientPlayerScoreElements[playerNetId];
            var player = _registeredPlayerControllers[playerNetId];

            scoreElement.gameObject.SetActive(false);

            player.ClientPlayerMetadataChanged -= PlayerMetadataChanged;
            
            _registeredPlayerControllers.Remove(playerNetId);
            _enabledClientPlayerScoreElements.Remove(playerNetId);
            
            _disabledPlayerScoreElements.Enqueue(scoreElement);
        }
    }
}