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
        [SerializeField] private DummyPlayersDataConfig _dummyPlayersDataConfig;

        [SerializeField] private Transform _modelLookingDirectionMark;
        [SerializeField] private Transform _characterModelTransform;
        [SerializeField] private Animator _animator;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private SpriteRenderer _lookingDirectionMarkRenderer;
        
        [SyncVar] private PlayerMetadata _playerMetadata;
        [SyncVar] private PlayerData _playerData;

        private Rigidbody _rigidbody;
        private PlayerMovingController _playerMovingController;
        private PlayerAnimationController _playerAnimationController;
        private PlayerInputController _playerInputController;
        private PlayerAppearanceController _playerAppearanceController;
        private DashAbility _dashAbility;
        
        private CancellationTokenSource _invincibilityStatusChangeCts;
        
        private HashSet<string> _invincibilityFlags = new();
        
        public Vector3 PlayerModelLookingDirection => _modelLookingDirectionMark.position - _characterModelTransform.position;
        
        public PlayerMetadata PlayerMetadata => _playerMetadata;
        public PlayerData PlayerData => _playerData;
        public bool IsInvincible => _invincibilityFlags.Count > 0;

        public event Action<PlayerController> InitializedAndSpawned; 

        public event Action<PlayerController, PlayerMetadata> ServerPlayerMetadataChanged; 
        public event Action<PlayerController, PlayerMetadata> ClientPlayerMetadataChanged;

        public void PrepareForSpawn(PlayerMetadata playerMetadata)
        {
            _playerMetadata = playerMetadata;
        }

        private void Start()
        {
            _playerData = _dummyPlayersDataConfig.CommonPlayerData;

            _rigidbody = GetComponent<Rigidbody>();
            
            _playerMovingController = new PlayerMovingController(_rigidbody);
            _playerAnimationController = new PlayerAnimationController(_animator);
            _playerAppearanceController = new PlayerAppearanceController(_meshRenderer, _lookingDirectionMarkRenderer);

            _dashAbility = new DashAbility(this, _playerMovingController, _playerAnimationController);

            _playerAppearanceController.SetColor(_playerMetadata.TeamColor);
            
            InitializedAndSpawned?.Invoke(this);
        }
        
        [Client]
        public void InitializeInput(Transform cameraTransform, Transform followerObjectTransform)
        {
            // Prevents input from local player to remote players
            if (isLocalPlayer)
            {
                _playerInputController = new PlayerInputController(this, cameraTransform, followerObjectTransform);
            }
            else
            {
                Debug.LogWarning("Trying to initialize input for remote player. This is not allowed.", this);
            }
        }

        [Client]
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
            
            _playerMovingController.Rotate(rotation, _playerData.RotationSpeed);
            
            _playerMovingController.Move(direction, _playerData.MovingSpeed, 
                ForceMode.VelocityChange, true, _playerData.MaxMoveSpeed);
            
            _playerAnimationController.AnimateMoving(true);
        }

        [Command]
        public void CommandDash(Vector3 direction)
        {
            _dashAbility.ServerDash(direction);
        }

        [ClientRpc]
        public void RpcOnDash(Vector3 direction, float power)
        {
            _dashAbility.ClientDash(direction, power);
        }
        
        [ClientRpc]
        public void RpcBlockMovement(bool block)
        {
            _playerData.CanMoveSelf = !block;
        }
        
        [ClientRpc]
        public void RpcBlockDash(bool block)
        {
            _playerData.CanDash = !block;
        }
        
        [ClientRpc]
        public void RpcSetDashPerforming(bool isPerforming)
        {
            _playerData.IsDashPerforming = isPerforming;
        }

        /// <summary>
        /// Called only for host because of <see cref="ServerCallbackAttribute"/>
        /// </summary>
        [ServerCallback]
        private void OnCollisionEnter(Collision collision)
        {
            if (this != null && collision.gameObject.CompareTag(ProjectConstants.PlayerTag))
            {
                var otherPlayer = collision.gameObject.GetComponent<PlayerController>();

                // If this player dashes in other player
                if (_dashAbility is {IsPerforming: true} && !otherPlayer.IsInvincible)
                {
                    _playerMetadata.Score += 1;
                    
                    RpcOnPlayerScoreChanged(_playerMetadata);
                    ServerPlayerMetadataChanged?.Invoke(this, _playerMetadata);
                    
                    otherPlayer.SetTemporarilyInvincible("DamageTaken", _playerData.InvincibilityAfterDamageTimeMillis);
                }
            }
        }

        [ClientRpc]
        private void RpcOnPlayerScoreChanged(PlayerMetadata playerMetadata)
        {
            ClientPlayerMetadataChanged?.Invoke(this, playerMetadata);
        }
        
        [ClientRpc]
        public void RpcSetInvincible(string context, bool isInvincible)
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
                ? _dummyPlayersDataConfig.CommonPlayerData.InvincibilityColor
                : _playerMetadata.TeamColor);
        }

        [Server]
        private void SetTemporarilyInvincible(string context, int millis)
        {
            _invincibilityStatusChangeCts?.Cancel();
            _invincibilityStatusChangeCts = new CancellationTokenSource();
            
            SetTempInvincibleProcess(context, millis, _invincibilityStatusChangeCts.Token).Forget();
        }

        /// <summary>
        /// Used to make player temporarily invincible.
        /// Notice that if cancelled, player will not be vulnerable again by the time end.
        /// </summary>
        [Server]
        private async UniTask SetTempInvincibleProcess(string context, int millis, CancellationToken cancellationToken)
        {
            if(cancellationToken.IsCancellationRequested) 
                return;

            RpcSetInvincible(context, true);

            await UniTask.Delay(millis, DelayType.DeltaTime, PlayerLoopTiming.FixedUpdate, cancellationToken);

            if(cancellationToken.IsCancellationRequested) 
                return;
            
            RpcSetInvincible(context, false);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            
            _playerInputController?.Dispose();
        }

        private void OnDestroy()
        {
            ServerPlayerMetadataChanged = null;
            ClientPlayerMetadataChanged = null;
            
            _invincibilityStatusChangeCts?.Dispose();
            _invincibilityStatusChangeCts = null;
            
            _playerAppearanceController?.Dispose();
            _playerMovingController?.Dispose();
            _playerInputController?.Dispose();

            _dashAbility?.Dispose();
        }
    }
}