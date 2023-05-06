using System;
using Game.Scripts.Globals;
using Game.Scripts.Player.Input;
using Game.Scripts.Player.ScriptableObjects;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("PhysicsCharacter Fields")] 
        [SerializeField] private Transform _lookingDirection;
        [SerializeField] private Transform _characterModelTransform;
        [SerializeField] private PlayerMovementConfig _playerMovementConfig;
        [SerializeField] private DummyPlayersDataConfig _dummyPlayersDataConfig;
        [SerializeField] private Animator _animator;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private SpriteRenderer _lookingDirectionMarkRenderer;

        private MovementSettingsData _runtimeMovementSettingsData;
        private PlayerData _playerData;

        private Rigidbody _rigidbody;
        private PlayerMovingController _playerMovingController;
        private PlayerAnimationController _playerAnimationController;
        private PlayerInputController _playerInputController;
        private PlayerAppearanceController _playerAppearanceController;
        private DashAbility _dashAbility;
        
        public Vector3 PlayerLookingDirection => _lookingDirection.position - _characterModelTransform.position;
        public PlayerData PlayerData => _playerData;

        public event Action<PlayerController> Initialized;

        protected void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _runtimeMovementSettingsData = _playerMovementConfig.MovementSettingsData;
            
            _playerMovingController = new PlayerMovingController(_rigidbody);

            _playerAnimationController = new PlayerAnimationController(_animator);
            
            _dashAbility = new DashAbility(true, this, 
                _playerMovingController, _playerAnimationController);

            _playerAppearanceController = new PlayerAppearanceController(_meshRenderer, 
                _lookingDirectionMarkRenderer);
            
            if (isLocalPlayer)
            {
                // Preventing input from local player to remote players
                _playerInputController = new PlayerInputController(this);
                
                _playerData = _dummyPlayersDataConfig.LocalPlayerData;
                _playerAppearanceController.SetColor(_dummyPlayersDataConfig.LocalPlayerData.TeamColor);
            }
            else
            {
                _playerData = _dummyPlayersDataConfig.RemotePlayerData;
                _playerAppearanceController.SetColor(_dummyPlayersDataConfig.RemotePlayerData.TeamColor);
            }
            
            Initialized?.Invoke(this);
        }

        private void OnDestroy()
        {
            Initialized = null;
            
            _playerInputController?.Dispose();
            _dashAbility?.Dispose();
            _playerAppearanceController?.Dispose();
        }

        [ServerCallback]
        private void OnCollisionEnter(Collision collision)
        {
            if (_dashAbility.IsPerforming && collision.gameObject.CompareTag(ProjectConstants.PlayerTag))
            {
                Debug.Log("Damage!");
            }
        }

        public void AttemptMoveSelf(Vector3 direction)
        {
            if (!_runtimeMovementSettingsData.CanMoveSelf) return;
            
            var b = direction.x;
            var a = direction.z;
            
            var rotationY = Mathf.Asin(a / Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2))) * 57.2957795131f;
            rotationY -= 90;
            
            if (b > 0)
            {
                rotationY *= -1;
            }

            var rotation = new Vector3(0, rotationY, 0);
            
            _playerMovingController.Rotate(_characterModelTransform, rotation, _runtimeMovementSettingsData.RotationSpeed);
            
            _playerMovingController.Move(direction, _runtimeMovementSettingsData.MovingSpeed, 
                ForceMode.VelocityChange, true, _runtimeMovementSettingsData.MaxMoveSpeed);
            
            _playerAnimationController.Animate(PlayerAnimationController.AnimationKey.Move);
        }

        public async void Dash(Vector3 direction)
        {
            if (!_dashAbility.IsAvailable) return;
            
            _runtimeMovementSettingsData.CanMoveSelf = false;
                
            await _dashAbility.Dash(direction, _runtimeMovementSettingsData.DashCooldownMillis, 
                _runtimeMovementSettingsData.DashSpeed);

            _runtimeMovementSettingsData.CanMoveSelf = true;
        }
    }
}