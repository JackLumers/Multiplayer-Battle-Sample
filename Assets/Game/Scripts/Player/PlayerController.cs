using Game.Scripts.Player.Input;
using Game.Scripts.Player.ScriptableObjects;
using UnityEngine;

namespace Game.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("PhysicsCharacter Fields")] 
        [SerializeField] private Transform _lookingDirection;
        [SerializeField] private Transform _characterModelTransform;
        [SerializeField] private Collider _movementCollider;
        [SerializeField] private PlayerMovementSettings _playerMovementSettings;
        [SerializeField] private Animator _animator;

        private MovementSettingsData _runtimeMovementSettingsData;

        private Rigidbody _rigidbody;
        private PlayerMovingController _playerMovingController;
        private PlayerAnimationController _playerAnimationController;
        private PlayerInputController _playerInputController;

        public Vector3 PlayerLookingDirection => _lookingDirection.position - _characterModelTransform.position;

        protected void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _runtimeMovementSettingsData = _playerMovementSettings.MovementSettingsData;
            _playerMovingController = new PlayerMovingController(_rigidbody, _movementCollider);

            _playerAnimationController = new PlayerAnimationController(_animator);

            _playerInputController = new PlayerInputController(this);
        }

        private void OnDestroy()
        {
            _playerInputController?.Dispose();
        }

        private void FixedUpdate()
        {
            _playerMovingController.FixedUpdateCallback();
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

        public void Dash(Vector3 direction)
        {
            // TODO: Dash distance instead of speed
            // var speed = _runtimeMovementSettingsData.DashDistance 
            
            Debug.Log(direction);
            
            _playerMovingController.Move(direction.normalized, _runtimeMovementSettingsData.DashSpeed, 
                ForceMode.Impulse, false);
            
            _playerAnimationController.Animate(PlayerAnimationController.AnimationKey.Dash);
        }
    }
}