using System.Collections.Generic;
using AbilitySystem.Attributes;
using AbilitySystem.Core;
using AbilitySystem.Effects;
using AbilitySystem.Targeting;
using UnityEngine;
using System.Linq;

using Attribute = AbilitySystem.Attributes.Attribute;
public class Enemy : MonoBehaviour,
IAttributeHolder,
IAbilityTarget,
IDamageable
{
    [SerializeField] private ConsumableAttributeDefinition _healthAttributeDefinition;
    private Dictionary<string, ConsumableAttribute> _consumableAttributes = new Dictionary<string, ConsumableAttribute>();
    private Dictionary<string, Attribute> _attributes = new Dictionary<string, Attribute>();
    
    void Awake()
    {
        RegisterAttributes();
        
    }
    
    public bool IsTargetable()
    {
        return true;
    }
    public void TakeDamage(float amount, ICaster source = null)
    {
        if (TryGetAttribute(_healthAttributeDefinition.AttributeName, out var healthAttribute)
            && healthAttribute is ConsumableAttribute consumableHealth)
        {
            
            consumableHealth.Consume(amount);
            Debug.Log($"{name} took {amount} damage. Remaining Health: {consumableHealth.CurrentAmount}");

            if(consumableHealth.CurrentAmount <300 && consumableHealth.RuntimeValue != 2000)
            {
                consumableHealth.SetRuntime(2000);
                OverTimeEffectLifetimeManager.Instance.ReApplyOverTimeEffectsModifier(this);
            }

            if (consumableHealth.CurrentAmount <= 0)
            {
                Die();
            }
        }
        else
        {
            Debug.LogError($"{name} could not resolve health attribute '{_healthAttributeDefinition.AttributeName}'.");
        }
    }
    private void Die()
    {
        Debug.Log($"{name} died!");
        // Add death logic here (e.g., play animation, drop loot, etc.)
    }

    public bool CanConsumeCost(IReadOnlyCollection<Attribute> costs)
    {
        foreach (var cost in costs)
        {  
            if (!TryGetAttribute(cost.Name, out var attribute) || !(attribute is ConsumableAttribute consumableAttribute) || consumableAttribute.CurrentAmount < cost.RuntimeValue)
            {
                return false;
            }
            
        }
        return true;
    }

    public void ConsumeCost(IReadOnlyCollection<Attribute> costs)
    {
        foreach (var cost in costs)
        {
            if (TryGetAttribute(cost.Name, out var attribute) && attribute is ConsumableAttribute consumableAttribute)
            {
                consumableAttribute.Consume(cost.RuntimeValue);
            }
        }
    }

    public bool TryGetAttribute(string attributeName, out Attribute attribute)
    {
        if (_consumableAttributes.TryGetValue(attributeName, out var consumableAttribute))
        {
            attribute = consumableAttribute;
            return true;
        }
        return _attributes.TryGetValue(attributeName, out attribute);
    }

    public void RegisterAttributes()
    {
        var health = _healthAttributeDefinition.CreateRuntimeConsumableAttribute();
        _consumableAttributes.Add(health.Name, health);
        Debug.Log($"Initialized resource: {health.Name} with MaxAmount: {health.RuntimeValue} and CurrentAmount: {health.CurrentAmount}");
    }

    
}
