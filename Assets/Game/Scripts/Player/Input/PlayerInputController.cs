using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.Globals;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Game.Scripts.Player.Input
{
    public class PlayerInputController : IDisposable
    {
        private PlayerController _playerController;
        private PlayerInputActions _playerInputActions;
        
        private Transform _cameraTransform;
        private Transform _followerObjectTransform;

        private CancellationTokenSource _checkingInputCst;

        private float _lookTargetYaw;
        private float _lookTargetPitch;
        
        public PlayerInputController(PlayerController playerController, 
            Transform cameraTransform, Transform followerObjectTransform)
        {
            _cameraTransform = cameraTransform;
            _followerObjectTransform = followerObjectTransform;
            
            _playerController = playerController;

            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Enable();

            _checkingInputCst = CancellationTokenSource
                .CreateLinkedTokenSource(playerController.GetCancellationTokenOnDestroy());
            
            PlayerInputFixedCheckProcess(_checkingInputCst.Token).Forget();
            PlayerInputUpdateCheckProcess(_checkingInputCst.Token).Forget();
            
            _playerInputActions.Player.Dash.performed += OnDashPerformed;
            
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

        private async UniTaskVoid PlayerInputFixedCheckProcess(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken: cancellationToken);
                
                Move();
            }
        }
        
        private async UniTaskVoid PlayerInputUpdateCheckProcess(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: cancellationToken);
                
                CameraLook();
            }
        }
        
        private void CameraLook()
        {
            var lookValue = _playerInputActions.Player.Look.ReadValue<Vector2>();
            
            if(lookValue.x == 0 && lookValue.y == 0)
                return;
            
            _lookTargetYaw += lookValue.x * InputConstants.CameraSensitivity * Time.deltaTime;
            _lookTargetPitch -= lookValue.y * InputConstants.CameraSensitivity * Time.deltaTime;

             // Clamp rotations so they limited to 360 degrees and pitch is limited to desired
             _lookTargetYaw = ClampAngle(_lookTargetYaw, float.MinValue, float.MaxValue);
             _lookTargetPitch = ClampAngle(_lookTargetPitch, CameraConstants.MaxDownLookDegrees, CameraConstants.MaxUpLookDegrees);

             _followerObjectTransform.rotation = Quaternion.Euler(_lookTargetPitch , _lookTargetYaw, 0.0f);
        }

        private void Move()
        {
            var moveValue = _playerInputActions.Player.Move.ReadValue<Vector2>();

            if (moveValue.x == 0 && moveValue.y == 0)
                return;

            var rotationRelativeToCamera = Mathf.Atan2(moveValue.x, moveValue.y) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            
            var direction = Quaternion.Euler(0.0f, rotationRelativeToCamera, 0.0f) * Vector3.forward;
            
            _playerController.AttemptMoveSelf(direction);
        }
        
        private void OnDashPerformed(InputAction.CallbackContext callbackContext)
        {
            _playerController.CommandDash(_playerController.PlayerModelLookingDirection);
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f) angle += 360f;
            if (angle > 360f) angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }
        
        public void Dispose()
        {
            _playerInputActions.Player.Dash.performed -= OnDashPerformed;
            InputUser.onChange -= OnControlDeviceChange;

            _playerInputActions?.Dispose();
            _checkingInputCst?.Cancel();
            _checkingInputCst?.Dispose();
            _checkingInputCst = null;
        }
    }
}