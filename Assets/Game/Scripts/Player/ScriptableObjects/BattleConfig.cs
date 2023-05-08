using System;
using System.Collections.Generic;
using Game.Scripts.Globals;
using UnityEngine;

namespace Game.Scripts.Player.ScriptableObjects
{
    [CreateAssetMenu(menuName = ProjectConstants.ScriptableObjectsAssetMenuName + "/Create new BattleConfig")]
    [Serializable]
    public class BattleConfig : ScriptableObject
    {
        [SerializeField] private int _maxScore;
        [SerializeField] private int _roundRestartSeconds;

        [SerializeField] private List<Color> _playerColors = new();
        
        public int MaxScore => _maxScore;
        public int RoundRestartSeconds => _roundRestartSeconds;

        private System.Random _random = new();
        
        /// <remarks>
        /// May be repeated
        /// </remarks>
        public Color GetRandomPlayerColor()
        {
            return _playerColors[_random.Next(_playerColors.Count)];
        }
    }
}