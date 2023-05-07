using Game.Scripts.Globals;
using UnityEngine;

namespace Game.Scripts.Player
{
    public class PlayerAnimationController
    {
        private Animator _playerAnimator;
        
        public PlayerAnimationController(Animator playerAnimator)
        {
            _playerAnimator = playerAnimator;
        }

        public void AnimateMoving(bool isMoving)
        {
            _playerAnimator.SetBool(AnimationConstants.IsMoving, isMoving);
        }
        
        public void AnimateDash()
        {
            _playerAnimator.SetTrigger(AnimationConstants.DashTrigger);
        }
    }
}