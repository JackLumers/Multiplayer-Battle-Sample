using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        public bool IsAvailable { get; private set; }
        public bool IsPerforming { get; private set; }
        
        private readonly PlayerController _playerController;
        private readonly PlayerMovingController _playerMovingController;
        private readonly PlayerAnimationController _playerAnimationController;
        
        private CancellationTokenSource _cts;

        public DashAbility(bool isAvailable, PlayerController playerController, 
            PlayerMovingController playerMovingController, PlayerAnimationController playerAnimationController)
        {
            IsAvailable = isAvailable;
            
            _playerController = playerController;
            _playerMovingController = playerMovingController;
            _playerAnimationController = playerAnimationController;

            _cts = CancellationTokenSource.CreateLinkedTokenSource(playerController.GetCancellationTokenOnDestroy());
        }
        
        public async UniTask Dash(Vector3 direction, int cooldownMillis, float distance)
        {
            _playerMovingController.Move(direction.normalized, distance, 
                ForceMode.Impulse, false);
            
            _playerAnimationController.Animate(PlayerAnimationController.AnimationKey.Dash);
            
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(_playerController.GetCancellationTokenOnDestroy());
            
            await Cooldown(cooldownMillis, _cts.Token);
        }
        
        /// <remarks>
        /// Cooldown time also acts as performing ("damage dealing") time.
        /// </remarks>
        private async UniTask Cooldown(int cooldownMillis, CancellationToken cancellationToken)
        {
            IsAvailable = false;
            IsPerforming = true;

            await UniTask.Delay(cooldownMillis, DelayType.DeltaTime, PlayerLoopTiming.FixedUpdate, cancellationToken);

            IsAvailable = true;
            IsPerforming = false;
        }

        public void Dispose()
        {
            _cts?.Dispose();
            _cts = null;
        }
    }
}