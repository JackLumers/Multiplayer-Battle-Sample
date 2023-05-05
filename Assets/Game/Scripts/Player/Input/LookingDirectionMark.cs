using UnityEngine;

namespace Game.Scripts.Player.Input
{
    public class LookingDirectionMark : MonoBehaviour
    {
        public Vector3 LookingDirection { get; set; }
        
        public void SetDirection(Vector3 lookingPoint)
        {
            LookingDirection = lookingPoint;
            gameObject.transform.LookAt(lookingPoint);
        }
    }
}