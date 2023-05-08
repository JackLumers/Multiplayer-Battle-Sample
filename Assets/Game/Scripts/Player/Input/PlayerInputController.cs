using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Game.Scripts.Player.Input
{
    public class PlayerInputController : IDisposable
    {
        private PlayerController _playerController;
        private PlayerInputActions _defaultInputActions;
        
        private CancellationTokenSource _checkingInputCst;
        
        public PlayerInputController(PlayerController playerController)
        {
            _playerController = playerController;

            _defaultInputActions = new PlayerInputActions();
            _defaultInputActions.Enable();

            _checkingInputCst = CancellationTokenSource
                .CreateLinkedTokenSource(playerController.GetCancellationTokenOnDestroy());
            
            PlayerMoveByInputProcess(_checkingInputCst.Token).Forget();
            
            _defaultInputActions.Player.Dash.performed += OnDashPerformed;
            
            InputUser.onChange += OnControlDeviceChange;
        }

        /// <summary>
        /// Checking control device change. Could be useful to adjust logic to other control schemes.
        /// </summary>
        private void OnControlDeviceChange(InputUser inputUser, InputUserChange inputUserChange, InputDevice inputDevice)
        {
            if (inputUserChange == InputUserChange.ControlSchemeChanged && inputUser.controlScheme.HasValue)
            { 
                Debug.Log($"[{GetType().Name}] Input control scheme changed. " +
                        $"New scheme: {inputUser.controlScheme.Value.name}", _playerController);
            }
        }

        private async UniTaskVoid PlayerMoveByInputProcess(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var value = _defaultInputActions.Player.Move.ReadValue<Vector2>();
                var asVector3 = new Vector3(value.x, 0, value.y);

                if (!asVector3.Equals(Vector3.zero))
                {
                    _playerController.AttemptMoveSelf(asVector3);
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken: cancellationToken);
            }
        }
        
        private void OnDashPerformed(InputAction.CallbackContext callbackContext)
        {
            _playerController.CommandDash(_playerController.PlayerLookingDirection);
        }
        
        public void Dispose()
        {
            _defaultInputActions.Player.Dash.performed -= OnDashPerformed;
            InputUser.onChange -= OnControlDeviceChange;

            _defaultInputActions?.Dispose();
            _checkingInputCst?.Dispose();
        }
    }
}