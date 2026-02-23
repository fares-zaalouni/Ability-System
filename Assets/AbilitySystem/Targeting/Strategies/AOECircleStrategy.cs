using System;
using System.Collections.Generic;
using UnityEngine;

public class AOECircleStrategy : ITargetingStrategy
{
    private Vector3 _center;
    private float _radius;
    public AOECircleStrategy(float radius)
    {
        _radius = radius;
    }
    public void UpdateCenter(Vector3 center)
    {
        _center = center;
    }
    public List<IAbilityTarget> GetTargets()
    {
        Collider[] colliders = Physics.OverlapSphere(_center, _radius);
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