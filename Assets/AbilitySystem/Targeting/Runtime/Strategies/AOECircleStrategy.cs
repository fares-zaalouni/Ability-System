using System;
using System.Collections.Generic;
using UnityEngine;

public class AOECircleStrategy : ITargetingStrategy
{
    private float _radius;
    public AOECircleStrategy(float radius)
    {
        _radius = radius;
    }
    public List<IAbilityTarget> GetTargets(AbilityContext context)
    {
        Vector3 center = context.TargetPoint;
        Collider[] colliders = Physics.OverlapSphere(center, _radius);
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