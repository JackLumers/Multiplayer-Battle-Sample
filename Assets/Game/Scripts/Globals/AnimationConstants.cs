using UnityEngine;

namespace Game.Scripts.Globals
{
    public class AnimationConstants
    {
        public static readonly int IsInvincible = Animator.StringToHash("IsInvincible");
        public static readonly int IsMoving = Animator.StringToHash("IsMoving");
        public static readonly int DashTrigger = Animator.StringToHash("DashTrigger");
        public static readonly int DeathTrigger = Animator.StringToHash("DeathTrigger");
    }
}