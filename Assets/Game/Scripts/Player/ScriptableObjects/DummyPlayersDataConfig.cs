using System;
using Game.Scripts.Globals;
using UnityEngine;

namespace Game.Scripts.Player.ScriptableObjects
{
    /// <summary>
    /// Contains info about players.
    /// Used to set initial local and remote player info, and could be used for test purposes.
    ///
    /// Theoretically could be parsed from some source, like tables, so
    /// game designers can adjust values from them.
    /// </summary>
    [CreateAssetMenu(menuName = ProjectConstants.ScriptableObjectsAssetMenuName + "/Create new DummyPlayerMetadataConfig")]
    [Serializable]
    public class DummyPlayersDataConfig : ScriptableObject
    {
        [SerializeField] private PlayerData _commonPlayerData;
        
        public PlayerData CommonPlayerData => _commonPlayerData;
    }

    [Serializable]
    public struct PlayerMetadata
    {
        [Header("Meta")] 
        public string Name;
        public Color TeamColor;
        public int Score;

        public PlayerMetadata(string name, Color teamColor, int score)
        {
            Name = name;
            TeamColor = teamColor;
            Score = score;
        }
    }

    [Serializable]
    public struct PlayerData
    {
        [Header("Movement")]
        // TODO: Can be a player state in state pattern
        public bool CanMoveSelf;
        public float RotationSpeed;
        public float MovingSpeed;
        public float MaxMoveSpeed;

        [Header("Dash")]
        // TODO: Can be a player state in state pattern
        public bool CanDash;
        public bool IsDashPerforming;
        
        public float DashPower;
        public int DashCooldownMillis;
        
        [Header("Invincibility")] 
        public int InvincibilityAfterDamageTimeMillis;
    }
}