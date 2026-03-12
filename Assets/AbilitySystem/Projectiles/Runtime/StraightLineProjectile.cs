using System;
using AbilitySystem.Core;
using AbilitySystem.Targeting;
using UnityEngine;

namespace AbilitySystem.Projectiles
{
    public class StraightLineProjectile : Projectile
    {      
        [SerializeField] private float _lifetime = 5f;
        private float _timeAlive = 0f;
        private bool _isLaunched = false;
        
        public override void Initialize(AbilityContext context)
        {
            
        }

        public override void Launch(Vector3 direction)
        {
            _direction = direction.normalized;
            _isLaunched = true;
        }

        protected override void TriggerHit(ProjectileHitData hitData)
        {
            base.TriggerHit(hitData);
        }

        private void Update()
        {
            if (!_isLaunched) return;
            transform.position += _direction * _speed * Time.deltaTime;
            _timeAlive += Time.deltaTime;
            if (_timeAlive >= _lifetime)
            {
                TriggerDestroy(new ProjectileDestroyData(transform.position, _direction.normalized));
                Destroy(gameObject);
            }
        }

        
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Projectile hit: {other.name}");
            IAbilityTarget target = other.GetComponent<IAbilityTarget>();
            if (target != null && target.IsTargetable())
            {
                RaycastHit hit;
                Vector3 normal = (_direction * -1f).normalized; // Default normal if raycast doesn't hit anything
                if (Physics.Raycast(transform.position - _direction * 0.1f, _direction, out hit, 0.2f))
                {
                    normal = hit.normal;
                }
                ProjectileHitData hitData = new ProjectileHitData(transform.position, normal, other);
                TriggerHit(hitData);
                if(_pierceCount > 0)
                {
                    _pierceCount--;
                }
                else
                {
                    TriggerDestroy(new ProjectileDestroyData(transform.position, _direction.normalized));
                    Destroy(gameObject);
                }
            }
        }
    }
}