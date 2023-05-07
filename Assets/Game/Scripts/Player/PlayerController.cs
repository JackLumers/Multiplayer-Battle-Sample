using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private Color _invincibilityColor;
        [SerializeField] private Transform _lookingDirection;
        [SerializeField] private Transform _characterModelTransform;
        [SerializeField] private DummyPlayersDataConfig _dummyPlayersDataConfig;
        [SerializeField] private Animator _animator;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private SpriteRenderer _lookingDirectionMarkRenderer;

        private MetaPlayerData _metaPlayerData;
        private PlayerData _playerData;
        
        private Rigidbody _rigidbody;
        private PlayerMovingController _playerMovingController;
        private PlayerAnimationController _playerAnimationController;
        private PlayerInputController _playerInputController;
        private PlayerAppearanceController _playerAppearanceController;
        private DashAbility _dashAbility;

        private CancellationTokenSource _invincibilityStatusChangeCts;
        private HashSet<string> _invincibilityFlags = new();

        public Vector3 PlayerLookingDirection => _lookingDirection.position - _characterModelTransform.position;
        public MetaPlayerData MetaPlayerData => _metaPlayerData;
        public bool IsInvincible => _invincibilityFlags.Count > 0;

        public event Action<PlayerController> Initialized;
        public event Action<PlayerController, MetaPlayerData> MetaDataChanged; 
        
        protected void Start()
        {
            _playerData = _dummyPlayersDataConfig.CommonPlayerData;
            
            _rigidbody = GetComponent<Rigidbody>();
            
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
                
                _metaPlayerData = _dummyPlayersDataConfig.LocalPlayerData;
                _playerAppearanceController.SetColor(_dummyPlayersDataConfig.LocalPlayerData.TeamColor);
            }
            else
            {
                _metaPlayerData = _dummyPlayersDataConfig.RemotePlayerData;
                _playerAppearanceController.SetColor(_dummyPlayersDataConfig.RemotePlayerData.TeamColor);
            }
            
            Initialized?.Invoke(this);
            
            _metaPlayerData.Changed += OnPlayerMetaDataChanged;
        }

        private void OnPlayerMetaDataChanged(MetaPlayerData obj)
        {
            MetaDataChanged?.Invoke(this, _metaPlayerData);
        }

        private void OnDestroy()
        {
            Initialized = null;
            MetaDataChanged = null;
            
            _metaPlayerData.Changed -= OnPlayerMetaDataChanged;
            
            _invincibilityStatusChangeCts?.Dispose();
            _invincibilityStatusChangeCts = null;
            
            _playerAppearanceController?.Dispose();
            _playerMovingController?.Dispose();
            _playerInputController?.Dispose();

            _dashAbility?.Dispose();
        }

        [ServerCallback]
        private void OnCollisionEnter(Collision collision)
        {
            if (this != null && _dashAbility is {IsPerforming: true} &&
                collision.gameObject.CompareTag(ProjectConstants.PlayerTag))
            {
                var otherPlayer = collision.gameObject.GetComponent<PlayerController>();
                if (!otherPlayer.IsInvincible)
                {
                    _metaPlayerData.Score += 1;

                    otherPlayer.SetTemporarilyInvincible("DamageTaken",
                        _playerData.InvincibilityAfterDamageTimeMillis);
                }
            }
        }

        public void AttemptMoveSelf(Vector3 direction)
        {
            if (!_playerData.CanMoveSelf) return;
            
            var b = direction.x;
            var a = direction.z;
            
            var rotationY = Mathf.Asin(a / Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2))) * 57.2957795131f;
            rotationY -= 90;
            
            if (b > 0)
            {
                rotationY *= -1;
            }

            var rotation = new Vector3(0, rotationY, 0);
            
            _playerMovingController.Rotate(_characterModelTransform, rotation, _playerData.RotationSpeed);
            
            _playerMovingController.Move(direction, _playerData.MovingSpeed, 
                ForceMode.VelocityChange, true, _playerData.MaxMoveSpeed);
            
            _playerAnimationController.AnimateMoving(true);
        }

        public async void Dash(Vector3 direction)
        {
            if (!_dashAbility.IsAvailable) return;
            
            _playerData.CanMoveSelf = false;
                
            await _dashAbility.Dash(direction, _playerData.DashCooldownMillis, 
                _playerData.DashSpeed);

            _playerData.CanMoveSelf = true;
        }

        public void SetInvincible(string context, bool isInvincible)
        {
            if (isInvincible)
            {
                if (!_invincibilityFlags.Contains(context)) _invincibilityFlags.Add(context);
            }
            else
            {
                if (_invincibilityFlags.Contains(context)) _invincibilityFlags.Remove(context);
            }

            _playerAppearanceController.SetColor(IsInvincible
                ? _invincibilityColor
                : _metaPlayerData.TeamColor);
        }

        public void SetTemporarilyInvincible(string context, int millis)
        {
            _invincibilityStatusChangeCts?.Cancel();
            _invincibilityStatusChangeCts = new CancellationTokenSource();
            
            SetTempInvincibleProcess(context, millis, _invincibilityStatusChangeCts.Token).Forget();
        }
        
        /// <summary>
        /// Used to make player temporarily invincible.
        /// Notice that if cancelled, player will not be vulnerable again by the time end.
        /// </summary>
        private async UniTask SetTempInvincibleProcess(string context, int millis, CancellationToken cancellationToken)
        {
            if(cancellationToken.IsCancellationRequested) 
                return;

            SetInvincible(context, true);

            await UniTask.Delay(millis, DelayType.DeltaTime, PlayerLoopTiming.FixedUpdate, cancellationToken);

            if(cancellationToken.IsCancellationRequested) 
                return;
            
            SetInvincible(context, false);
        }
    }
}