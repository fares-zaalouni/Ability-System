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
                if (!context.TryGet<ProjectileHitData>(out var hitData))
                    return new List<IAbilityTarget>();

                if (hitData.HitObject != null)
                {
                    IAbilityTarget target = hitData.HitObject.GetComponent<IAbilityTarget>();
                    if (target != null && target.IsTargetable())
                    {
                        return new List<IAbilityTarget> { target };
                    }
                }
            }
            else
            {
                if (context.TryGet<TargetPoint>(out var targetPointData))
                {
                    Vector3 targetPoint = targetPointData.Value;
                   
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