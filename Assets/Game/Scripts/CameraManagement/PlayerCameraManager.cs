using Cinemachine;
using Game.Scripts.Player;
using Mirror;
using UnityEngine;

namespace Game.Scripts.CameraManagement
{
    public class PlayerCameraManager : NetworkBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _playerCamera;
        [SerializeField] private FollowerObject _playerFollowerObject;

        private PlayerController _playerController;

        [ClientRpc]
        public void RpcAttachPlayer(PlayerController playerController)
        {
            Debug.Log($"RpcAttachPlayer! ");
            
            if (!playerController.isLocalPlayer) return;
            
            _playerController = playerController;
            
            _playerFollowerObject.FollowTransform = playerController.transform;
            _playerFollowerObject.gameObject.SetActive(true);
            
            _playerCamera.gameObject.SetActive(true);
            
            playerController.InitializeInput(_playerCamera.transform, _playerFollowerObject.transform);
        }

        [ClientRpc]
        public void RpcDetachPlayer()
        {
            Debug.Log($"RpcAttachPlayer! {_playerController.isLocalPlayer}");

            if(_playerController.isLocalPlayer)
                ClientDetachPlayer();
        }

        private void ClientDetachPlayer()
        {
            _playerFollowerObject.FollowTransform = null;
            _playerFollowerObject.gameObject.SetActive(false);
            
            _playerCamera.gameObject.SetActive(false);
        }
        
        public override void OnStopClient()
        {
            ClientDetachPlayer();
        }
    }
}