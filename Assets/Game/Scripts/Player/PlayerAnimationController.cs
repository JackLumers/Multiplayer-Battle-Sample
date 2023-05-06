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

        public void Animate(AnimationKey key)
        {
            // TODO: Animate here
        }
        
        public enum AnimationKey
        {
            Move,
            Dash,
            Death,
            DamageTaken
        }
    }
}