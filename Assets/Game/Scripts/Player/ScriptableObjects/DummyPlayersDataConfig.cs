using System;
using Game.Scripts.Globals;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Player.ScriptableObjects
{
    /// <summary>
    /// Contains info about players.
    /// Used just to set local and remote player info, and could be used for test purposes.
    ///
    /// Theoretically could be parsed from some source, like tables, so
    /// game designers can adjust values there.
    /// </summary>
    [CreateAssetMenu(menuName = ProjectConstants.ScriptableObjectsAssetMenuName + "/Create new DummyPlayerMetadataConfig")]
    [Serializable]
    public class DummyPlayersDataConfig : ScriptableObject
    {
        [SerializeField] private PlayerData _commonPlayerData;
        [SerializeField] private MetaPlayerData _localPlayerData;
        [SerializeField] private MetaPlayerData _remotePlayerData;
        
        public MetaPlayerData LocalPlayerData => _localPlayerData;
        public MetaPlayerData RemotePlayerData => _remotePlayerData;
        public PlayerData CommonPlayerData => _commonPlayerData;
    }

    [Serializable]
    public struct MetaPlayerData
    {
        [Header("Meta")] 
        [SyncVar] [SerializeField] private string _name;
        [SyncVar] [SerializeField] private Color _teamColor;
        [SyncVar] [SerializeField] private int _score;

        public event Action<MetaPlayerData> Changed;

        public int Score
        {
            get => _score;

            [Command]
            set
            {
                _score = value;
                Changed?.Invoke(this);
            }
        }

        public string Name => _name;
        public Color TeamColor => _teamColor;
    }

    [Serializable]
    public struct PlayerData
    {
        [Header("Flags")]
        public bool CanMoveSelf;

        [Header("Movement")]
        public float RotationSpeed;
        public float MovingSpeed;
        public float MaxMoveSpeed;
        
        [Header("Dash")]
        // TODO: Change to distance
        public float DashSpeed;
        public int DashCooldownMillis;
        
        [Header("Invincibility")] 
        public int InvincibilityAfterDamageTimeMillis;
    }
}