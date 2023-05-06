using System.Collections.Generic;
using Game.Scripts.Player;
using Game.Scripts.Player.ScriptableObjects;
using UnityEngine;

namespace Game.Scripts.UI
{
    [RequireComponent(typeof(Canvas))]
    public class GameUiWindow : MonoBehaviour
    {
        [SerializeField] private Transform _scoreElementsLayout;
        [SerializeField] private PlayerScoreElement _playerScoreElementPrefab;
        
        // Can be taken from Pooling System + Addressable Assets Async Instantiation,
        // but for sake of simplicity will do.
        private Queue<PlayerScoreElement> _disabledPlayerScoreElements = new();
        
        // Key = netId
        private Dictionary<uint, PlayerController> _registeredPlayerControllers = new();
        private Dictionary<uint, PlayerScoreElement> _playerScoreElements = new();
        
        public void RegisterPlayer(PlayerController playerController, PlayerData playerData)
        {
            if (_registeredPlayerControllers.ContainsKey(playerController.netId)) 
                return;

            if (!_disabledPlayerScoreElements.TryDequeue(out var playerScoreElement))
            {
                playerScoreElement = Instantiate(_playerScoreElementPrefab, _scoreElementsLayout);
                playerScoreElement.gameObject.SetActive(false);
            }
            
            _registeredPlayerControllers.Add(playerController.netId, playerController);
            _playerScoreElements.Add(playerController.netId, playerScoreElement);

            playerScoreElement.SetScore(playerData.Name, playerData.Score.ToString());
            playerScoreElement.SetBackgroundColor(playerData.TeamColor);
            
            playerScoreElement.gameObject.SetActive(true);
        }

        public void UnregisterPlayer(uint netId)
        {
            if (!_registeredPlayerControllers.ContainsKey(netId)) 
                return;
            
            var scoreElement = _playerScoreElements[netId];
            scoreElement.gameObject.SetActive(false);
            
            _registeredPlayerControllers.Remove(netId);
            _playerScoreElements.Remove(netId);
            
            _disabledPlayerScoreElements.Enqueue(scoreElement);
        }
    }
}