using System;
using DG.Tweening;
using UnityEngine;

namespace Game.Scripts.Player
{
    [Serializable]
    public class PlayerMovingController : IDisposable
    {
        private Rigidbody _rigidbody;
        
        private Tweener _rotationTween;

        public PlayerMovingController(Rigidbody rigidbody)
        {
            _rigidbody = rigidbody;
        }

        /// <summary>
        /// Moves player by applying force.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="speedMultiplier"></param>
        /// <param name="forceMode"><see cref="ForceMode"/></param>
        /// <param name="smoothByTimeDelta">Do use <see cref="Time.deltaTime"/> for smoothing?</param>
        /// <param name="maxSpeed">Clamps if set. -1 means no clamp.</param>
        public void Move(Vector3 direction, float speedMultiplier, ForceMode forceMode, 
            bool smoothByTimeDelta, float maxSpeed = -1)
        {
            direction.Normalize();

            var force = direction * speedMultiplier;
            
            if (smoothByTimeDelta)
                force *= Time.deltaTime;
            
            _rigidbody.AddForce(force, forceMode);

            if (maxSpeed >= 0)
            {
                ClampVelocity(maxSpeed);
            }
        }

        public void Rotate(Transform characterModelTransform, Vector3 rotation, float speedMultiplier)
        {
            _rotationTween?.Kill();
            _rotationTween = characterModelTransform
                .DORotate(rotation, speedMultiplier)
                .SetSpeedBased(true);
        }

        private void ClampVelocity(float maxVelocity)
        {
            _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, maxVelocity);
        }

        public void Dispose()
        {
            _rotationTween?.Kill();
        }
    }
}