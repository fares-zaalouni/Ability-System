using System;
using UnityEngine;

namespace AbilitySystem.Projectiles
{
    public abstract class Projectile : MonoBehaviour
    {
        public event Action<ProjectileHitData> OnHit;

        public abstract void Launch(Vector3 direction, float speed);
        protected void Hit(ProjectileHitData hitData)
        {
            OnHit?.Invoke(hitData);
        }
        
        
    }
}