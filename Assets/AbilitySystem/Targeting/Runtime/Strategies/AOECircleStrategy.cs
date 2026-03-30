using System.Collections.Generic;
using UnityEngine;
using AbilitySystem.Core;

namespace AbilitySystem.Targeting
{
    public class AOECircleStrategy : ITargetingStrategy
    {
        private float _radius;
        private LayerMask _targetLayerMask;
        public AOECircleStrategy(float radius, LayerMask targetLayerMask)
        {
            _radius = radius;
            _targetLayerMask = targetLayerMask;
        }
        public List<IAbilityTarget> GetTargets(AbilityContext context)
        {
            if (!context.TryGet<TargetPoint>(out var targetPoint))
                return new List<IAbilityTarget>();

            Vector3 center = targetPoint.Value;

            Collider[] colliders = Physics.OverlapSphere(center, _radius, _targetLayerMask);
            List<IAbilityTarget> targets = new List<IAbilityTarget>();
            foreach (Collider collider in colliders)
            {
                IAbilityTarget target = collider.GetComponent<IAbilityTarget>();
                if (target != null && target.IsTargetable())
                {
                    targets.Add(target);
                }
            }
            return targets;
        }
    }
}