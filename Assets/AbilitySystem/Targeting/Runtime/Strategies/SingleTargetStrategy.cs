using System.Collections.Generic;
using AbilitySystem.Core;
using AbilitySystem.Projectiles;
using UnityEngine;

namespace AbilitySystem.Targeting
{
    public class SingleTargetStrategy : ITargetingStrategy
    {
        private LayerMask _targetLayerMask;
        private float _precisionRadius;
        private bool IsProjectileHit;
        public SingleTargetStrategy(LayerMask targetLayerMask, float precisionRadius, bool isProjectileHit = false)
        {
            _targetLayerMask = targetLayerMask;
            _precisionRadius = precisionRadius;
            IsProjectileHit = isProjectileHit;
        }
        public List<IAbilityTarget> GetTargets(AbilityContext context)
        {
            if(IsProjectileHit)
            {
                if(context.TryGet(ContextKeys.ProjectileHitData, out ProjectileHitData hitData))
                {
                    if (hitData.HitObject != null)
                    {
                        IAbilityTarget target = hitData.HitObject.GetComponent<IAbilityTarget>();
                        if (target != null && target.IsTargetable())
                        {
                            return new List<IAbilityTarget> { target };
                        }
                    }
                }
            }
            else
            {
                if(context.TryGet(ContextKeys.TargetPoint, out Vector3 targetPoint))
                {
                   
                    Collider[] hits = Physics.OverlapSphere(targetPoint, _precisionRadius, _targetLayerMask);
                    if (hits.Length > 0)
                    {
                        IAbilityTarget abilityTarget;

                        foreach (Collider hit in hits)
                        {
                            abilityTarget = hit.GetComponent<IAbilityTarget>();
                            if (abilityTarget != null)
                            {
                                return new List<IAbilityTarget> { abilityTarget };
                            }
                        }
                    }
                }
            }
            return new List<IAbilityTarget>();
        }
    }
}