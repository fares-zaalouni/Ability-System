
using System;
using System.Collections.Generic;
using AbilitySystem.Core;
using UnityEngine;

public class FireBall: MonoBehaviour
{
    [SerializeField] private AbilityDefinition _abilityDefinition;
    private AbilityInstance _abilityInstance;

    public void Initialize(ICaster caster)
    {
        _abilityInstance = new AbilityInstance(_abilityDefinition, caster);
    }

    public void CastFireBall()
    {
        
        if (_abilityInstance != null)
        {
            _abilityInstance.Cast();
        }
    }
}