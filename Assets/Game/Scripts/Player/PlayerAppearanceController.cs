using System;
using Mirror;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Scripts.Player
{
    public class PlayerAppearanceController : IDisposable
    {
        private readonly SpriteRenderer _lookingDirectionMarkRenderer;
        
        // For optimization better use material properties, but there
        // is no custom shader for player so this will do the work.
        private readonly Material _material;

        public PlayerAppearanceController(Renderer renderer, 
            SpriteRenderer lookingDirectionMarkRenderer)
        {
            _lookingDirectionMarkRenderer = lookingDirectionMarkRenderer;
            
            _material = renderer.material;
        }

        public void SetColor(Color color)
        {
            _material.color = color;

            _lookingDirectionMarkRenderer.color = color;
        }

        public void Dispose()
        {
            Object.Destroy(_material);
        }
    }
}