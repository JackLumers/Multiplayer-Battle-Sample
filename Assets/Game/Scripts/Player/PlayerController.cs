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
        [SerializeField] private PlayerMovementSettings _playerMovementSettings;
        [SerializeField] private Animator _animator;

        private MovementSettingsData _runtimeMovementSettingsData;

        private Rigidbody _rigidbody;
        private PlayerMovingController _playerMovingController;
        private PlayerAnimationController _playerAnimationController;
        private PlayerInputController _playerInputController;
        private DashAbility _dashAbility;
        
        public Vector3 PlayerLookingDirection => _lookingDirection.position - _characterModelTransform.position;

        protected void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _runtimeMovementSettingsData = _playerMovementSettings.MovementSettingsData;
            
            _playerMovingController = new PlayerMovingController(_rigidbody);

            _playerAnimationController = new PlayerAnimationController(_animator);

            // Preventing input from local player to remote players
            if(isLocalPlayer) 
                _playerInputController = new PlayerInputController(this);

            _dashAbility = new DashAbility(true, this, 
                _playerMovingController, _playerAnimationController);
        }

        private void OnDestroy()
        {
            _playerInputController?.Dispose();
            _dashAbility?.Dispose();
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