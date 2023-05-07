using System.Collections.Generic;
using Game.Scripts.Player;
using Game.Scripts.Player.ScriptableObjects;
using Mirror;
using UnityEngine;

namespace Game.Scripts.UI
{
    [RequireComponent(typeof(Canvas))]
    public class GameUiWindow : MonoBehaviour
    {
        [SerializeField] private Transform _scoreElementsLayout;
        [SerializeField] private RoundResultElement _roundResultElement;
        [SerializeField] private PlayerScoreElement _playerScoreElementPrefab;
        
        // Can be taken from Pooling System + Addressable Assets Async Instantiation,
        // but for sake of simplicity will do.
        private Queue<PlayerScoreElement> _disabledPlayerScoreElements = new();
        
        // Key = connection id
        private Dictionary<int, PlayerController> _registeredPlayerControllers = new();
        private Dictionary<int, PlayerScoreElement> _playerScoreElements = new();
        
        public void RegisterPlayer(PlayerController player, MetaPlayerData playerData)
        {
            var connectionId = player.connectionToClient.connectionId;
            
            if (_registeredPlayerControllers.ContainsKey(connectionId))
                return;

            if (!_disabledPlayerScoreElements.TryDequeue(out var playerScoreElement))
            {
                playerScoreElement = Instantiate(_playerScoreElementPrefab, _scoreElementsLayout);
                playerScoreElement.gameObject.SetActive(false);
            }
            
            _registeredPlayerControllers.Add(connectionId, player);
            _playerScoreElements.Add(connectionId, playerScoreElement);

            playerScoreElement.SetData(playerData);

            player.MetaDataChanged += OnPlayerMetaChanged;
            
            playerScoreElement.gameObject.SetActive(true);
        }

        public void UnregisterPlayer(int connectionId)
        {
            if (!_registeredPlayerControllers.ContainsKey(connectionId)) 
                return;
            
            var scoreElement = _playerScoreElements[connectionId];
            var player = _registeredPlayerControllers[connectionId];

            scoreElement.gameObject.SetActive(false);

            player.MetaDataChanged -= OnPlayerMetaChanged;
            
            _registeredPlayerControllers.Remove(connectionId);
            _playerScoreElements.Remove(connectionId);
            
            _disabledPlayerScoreElements.Enqueue(scoreElement);
        }
        
        private void OnPlayerMetaChanged(PlayerController playerController, MetaPlayerData newData)
        {
            if (_playerScoreElements.TryGetValue(playerController.connectionToClient.connectionId,
                    out var playerScoreElement))
            {
                playerScoreElement.SetData(newData);
            }
        }

        public void ShowGameResultAndRestartTimer(MetaPlayerData winnerData, int restartMillis)
        {
            _roundResultElement.SetWinnerData(winnerData);
            _roundResultElement.StartVisualTimer(restartMillis).Forget();
            
            _roundResultElement.gameObject.SetActive(true);
        }

        public void HideGameResult()
        {
            _roundResultElement.gameObject.SetActive(false);
        }
    }
}