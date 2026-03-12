using System;
using AbilitySystem.Core;
using UnityEngine;

namespace AbilitySystem.Projectiles
{
    public abstract class Projectile : MonoBehaviour
    {
        protected Vector3 _direction;
        [SerializeField] protected float _speed;
        [SerializeField] protected uint _pierceCount = 0;
        public event Action<ProjectileHitData> OnHit;
        public event Action<ProjectileDestroyData> OnDestroyed;

        public abstract void Initialize(AbilityContext context);

        public abstract void Launch(Vector3 direction);

        protected virtual void TriggerHit(ProjectileHitData hitData)
        {
            OnHit?.Invoke(hitData);
        }

        protected virtual void TriggerDestroy(ProjectileDestroyData destroyData)
        {
            OnDestroyed?.Invoke(destroyData);
        }
    }
}