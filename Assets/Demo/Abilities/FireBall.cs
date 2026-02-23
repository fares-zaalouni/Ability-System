
using System;
using System.Collections.Generic;
using UnityEngine;

public class FireBall: MonoBehaviour
{
    [SerializeField] private AbilityDefinition _abilityDefinition;
    [SerializeField] private float _radius;
    [SerializeField] private float _range;
    private AbilityInstance _abilityInstance;
    private AOECircleStrategy _targetingStrategy;

    public void Initialize(ICaster caster)
    {
        AOECircleStrategy targetingStrategy = new AOECircleStrategy(_radius);
        _targetingStrategy = targetingStrategy;
        _abilityInstance = new AbilityInstance(_abilityDefinition, caster, targetingStrategy);
    }

    public void CastFireBall(Vector3 targetPoint)
    {
        Debug.Log($"Casting FireBall at {targetPoint}");
        
        if (_abilityInstance != null)
        {
            _targetingStrategy.UpdateCenter(targetPoint);
            _abilityInstance.Cast();
        }
    }
}