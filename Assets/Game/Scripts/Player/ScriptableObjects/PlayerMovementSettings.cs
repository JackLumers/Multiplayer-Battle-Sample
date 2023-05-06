using System;
using Game.Scripts.Globals;
using UnityEngine;

namespace Game.Scripts.Player.ScriptableObjects
{
    [CreateAssetMenu(menuName = ProjectConstants.ScriptableObjectsAssetMenuName + "/Create new PlayerMovementSettings")]
    [Serializable]
    public class PlayerMovementSettings : ScriptableObject
    {
        [SerializeField] private MovementSettingsData _movementSettingsData;

        public MovementSettingsData MovementSettingsData => _movementSettingsData;
    }

    [Serializable]
    public struct MovementSettingsData
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
    }
}