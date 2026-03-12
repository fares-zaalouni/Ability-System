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
            if(!context.TryGet(ContextKeys.AOECenter, out Vector3 center))
            {
                Debug.LogError("AOECircleStrategy requires an AOECenter in the context.");
                return new List<IAbilityTarget>();
            }

            Collider[] colliders = Physics.OverlapSphere(center, _radius, _targetLayerMask);
            List<IAbilityTarget> targets = new List<IAbilityTarget>();
            foreach (Collider collider in colliders)
            {
                IAbilityTarget target = collider.GetComponent<IAbilityTarget>();
                Debug.Log($"Checking collider {collider.name} for IAbilityTarget. Found: {target != null}");
                if (target != null && target.IsTargetable())
                {
                    targets.Add(target);
                }
            }
            return targets;
        }
    }
}