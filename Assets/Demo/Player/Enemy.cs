using System;
using System.Collections.Generic;
using AbilitySystem.Attributes;
using AbilitySystem.Core;
using AbilitySystem.Targeting;
using UnityEngine;

using Attribute = AbilitySystem.Attributes.Attribute;
public class Enemy : MonoBehaviour,
IAbilityTarget,
IDamageable
{
    [SerializeField] private ConsumableAttributeDefinition _healthAttributeDefinition;
    private ConsumableAttribute _healthAttribute;
    
    void Awake()
    {
        RegisterResources();
    }
    public void RegisterResources()
    {
        _healthAttribute = _healthAttributeDefinition.CreateRuntimeConsumableAttribute();
    }
    public bool IsTargetable()
    {
        return true;
    }
    public void TakeDamage(float amount, ICaster source = null)
    {
        if (_healthAttribute != null)
        {
            _healthAttribute.Consume(amount);
            Debug.Log($"{name} took {amount} damage. Remaining Health: {_healthAttribute.CurrentAmount}");
            if (_healthAttribute.CurrentAmount <= 0)
            {
                Die();
            }
        }
    }
    private void Die()
    {
        Debug.Log($"{name} died!");
        // Add death logic here (e.g., play animation, drop loot, etc.)
    }

}
