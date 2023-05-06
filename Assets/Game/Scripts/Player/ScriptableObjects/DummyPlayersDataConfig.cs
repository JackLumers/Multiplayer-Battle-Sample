using System;
using Game.Scripts.Globals;
using UnityEngine;

namespace Game.Scripts.Player.ScriptableObjects
{
    /// <summary>
    /// Contains info about players, like name, color and score.
    /// Used just to set local and remote player info, and could be used to set score for test purposes.
    /// </summary>
    [CreateAssetMenu(menuName = ProjectConstants.ScriptableObjectsAssetMenuName + "/Create new DummyPlayerMetadataConfig")]
    [Serializable]
    public class DummyPlayersDataConfig : ScriptableObject
    {
        [SerializeField] private PlayerData _localPlayerData;
        [SerializeField] private PlayerData _remotePlayerData;
        
        public PlayerData LocalPlayerData => _localPlayerData;
        public PlayerData RemotePlayerData => _remotePlayerData;
    }

    [Serializable]
    public struct PlayerData
    {
        public string Name;
        public Color TeamColor;
        public int Score;
    }
}