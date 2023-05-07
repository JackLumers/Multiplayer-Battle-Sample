using System;
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
        
        public int MaxScore => _maxScore;
        public int RoundRestartSeconds => _roundRestartSeconds;
    }
}