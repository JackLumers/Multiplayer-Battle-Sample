using UnityEngine;

namespace Game.Scripts.Globals
{
    public static class AnimationConstants
    {
        public static readonly int IsMoving = Animator.StringToHash("IsMoving");
        public static readonly int DashTrigger = Animator.StringToHash("DashTrigger");
    }
}