using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using UnityEngine;

namespace Game.Scripts.Player
{
    /// <summary>
    /// Describes player's dash ability.
    /// 
    /// <remarks>
    /// Could be inherited from some base ability class,
    /// but since there is only dash ability exist I decided to leave it as it is for simplicity.
    /// </remarks>
    /// </summary>
    public class DashAbility : IDisposable
    {
        public bool IsPerforming => _playerController.PlayerData.IsDashPerforming;
        public bool IsAvailable => _playerController.PlayerData.CanDash;
        
        private readonly PlayerController _playerController;
        private readonly PlayerMovingController _playerMovingController;
        private readonly PlayerAnimationController _playerAnimationController;
        
        private CancellationTokenSource _dashCts;

        public DashAbility(PlayerController playerController, 
            PlayerMovingController playerMovingController, PlayerAnimationController playerAnimationController)
        {
            _playerController = playerController;
            _playerMovingController = playerMovingController;
            _playerAnimationController = playerAnimationController;
        }
        
        [Server]
        public void ServerDash(Vector3 direction)
        {
            _dashCts?.Cancel();
            _dashCts = new CancellationTokenSource();
            
            ServerDashProcess(direction, _dashCts.Token).Forget();
        }
        
        [Server]
        private async UniTaskVoid ServerDashProcess(Vector3 direction, CancellationToken cancellationToken)
        {
            Debug.Log($"Command dash call. NetId: {_playerController.netId}, " +
                      $"Is dash available: {IsAvailable}");
            
            if (!IsAvailable) 
                return;
            
            _playerController.RpcBlockMovement(true);
            _playerController.RpcBlockDash(true);
            _playerController.RpcSetDashPerforming(true);

            _playerController.RpcOnDash(direction, _playerController.PlayerData.DashPower);

            // Cooldown and duration
            await UniTask.Delay(_playerController.PlayerData.DashCooldownMillis, 
                    DelayType.DeltaTime, PlayerLoopTiming.FixedUpdate, cancellationToken)
                .SuppressCancellationThrow();
            
            // Check because player can already be destroyed after cancellation
            // TODO: Can be optimized using 2 cancellation tokens, since Null check is more expensive
            if (_playerController != null)
            {
                _playerController.RpcBlockMovement(false);
                _playerController.RpcBlockDash(false);
                _playerController.RpcSetDashPerforming(false);
            }
        }
        
        [Client]
        public void ClientDash(Vector3 direction, float power)
        {
            _playerMovingController.Move(direction.normalized, power, 
                ForceMode.Impulse, false);
            
            _playerAnimationController.AnimateDash();
        }

        public void Dispose()
        {
            _dashCts?.Dispose();
            _dashCts = null;
        }
    }
}