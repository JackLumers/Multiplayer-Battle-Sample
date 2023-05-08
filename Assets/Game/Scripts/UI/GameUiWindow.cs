using System.Collections.Generic;
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
        
        // Can be taken from Pooling System + Addressable Assets Async Instantiation,
        // but for sake of simplicity will do.
        private Queue<PlayerScoreElement> _disabledPlayerScoreElements = new();
        
        // Key = NetId
        private Dictionary<uint, PlayerController> _registeredPlayerControllers = new();
        private Dictionary<uint, PlayerScoreElement> _playerScoreElements = new();
        
        [Server]
        public void RegisterPlayer(PlayerController player, PlayerMetadata playerMetadata)
        {
            Debug.Log($"Server RegisterPlayer! Name: {playerMetadata.Name}", this);
            RpcAddPlayerScoreElement(player, playerMetadata);
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
        private void RpcAddPlayerScoreElement(PlayerController player, PlayerMetadata playerMetadata)
        {
            Debug.Log($"RpcAddPlayerScoreElement! Name: {playerMetadata.Name}", this);
            var connectionId = player.netId;
            
            if (_registeredPlayerControllers.ContainsKey(connectionId))
                return;

            if (!_disabledPlayerScoreElements.TryDequeue(out var playerScoreElement))
            {
                playerScoreElement = Instantiate(_playerScoreElementPrefab, _scoreElementsLayout);

                playerScoreElement.gameObject.SetActive(false);
            }

            _registeredPlayerControllers.Add(connectionId, player);
            _playerScoreElements.Add(connectionId, playerScoreElement);

            playerScoreElement.SetData(playerMetadata);

            player.ClientPlayerMetadataChanged += PlayerMetadataChanged;

            playerScoreElement.gameObject.SetActive(true);
        }

        [ClientRpc]
        private void RpcRemovePlayerScoreElement(uint playerNetId)
        {
            if (!_registeredPlayerControllers.ContainsKey(playerNetId)) 
                return;
            
            var scoreElement = _playerScoreElements[playerNetId];
            var player = _registeredPlayerControllers[playerNetId];

            scoreElement.gameObject.SetActive(false);

            player.ClientPlayerMetadataChanged -= PlayerMetadataChanged;
            
            _registeredPlayerControllers.Remove(playerNetId);
            _playerScoreElements.Remove(playerNetId);
            
            _disabledPlayerScoreElements.Enqueue(scoreElement);
        }
        
        [Client]
        private void PlayerMetadataChanged(PlayerController playerController, PlayerMetadata newMetadata)
        {
            if (_playerScoreElements.TryGetValue(playerController.netId,
                    out var playerScoreElement))
            {
                playerScoreElement.SetData(newMetadata);
            }
        }
    }
}