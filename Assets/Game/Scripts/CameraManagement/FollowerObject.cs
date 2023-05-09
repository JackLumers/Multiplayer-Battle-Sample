using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Scripts.CameraManagement
{
    public class FollowerObject : MonoBehaviour
    {
        private Transform _transform;
        
        [CanBeNull] [NonSerialized]
        public Transform FollowTransform;

        private void Awake()
        {
            _transform = transform;
        }
        
        private void LateUpdate()
        {
            if (!ReferenceEquals(FollowTransform, null) && !ReferenceEquals(_transform, null))
            { 
                _transform.position = FollowTransform.position;
            }
        }
    }
}