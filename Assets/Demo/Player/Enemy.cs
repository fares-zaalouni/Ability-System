using System;
using System.Collections.Generic;
using AbilitySystem.Core;
using AbilitySystem.Effects;
using AbilitySystem.Resources;
using AbilitySystem.Targeting;
using UnityEngine;

public class Enemy : StatusEffectReceiver,
IAbilityTarget,
IDamageable,
IResourceBearer
{
    [SerializeField] private List<ResourceDefinition> _resourceDefinitions = new List<ResourceDefinition>();
    private Dictionary<string, IResource> _resources = new Dictionary<string, IResource>();
    void Awake()
    {
        RegisterResources();
    }
    public void RegisterResources()
    {
        foreach (var resourceDef in _resourceDefinitions)
        {
            var runtimeResource = resourceDef.CreateRuntimeResource();
            _resources.Add(runtimeResource.Name, runtimeResource);
            foreach(var resource in _resources.Values)
            {
                Debug.Log($"Initialized resource: {resource.Name} with MaxAmount: {resource.MaxAmount}");
            }
        }
    }
    public bool IsTargetable()
    {
        return true;
    }
    public bool CanApplyEffect(IAbilityEffect effect)
    {
        if (effect is DamageEffect || effect is DOTEffect)
        {
            return true;
        }
        return false;
    }
    public void TakeDamage(float amount, IResourceBearer source = null)
    {
        if (_resources.TryGetValue("Health", out var healthResource))
        {
            healthResource.Consume(amount); 
            Debug.Log($"Enemy took {amount} damage. Remaining Health: {healthResource.CurrentAmount}");
            if (healthResource.CurrentAmount <= 0)
            {
                Die();
            }
        }
    }
    private void Die()
    {
        Debug.Log("Enemy died!");
        // Add death logic here (e.g., play animation, drop loot, etc.)
    }

    public bool CanConsumeCost(IReadOnlyCollection<Cost> costs)
    {
        throw new NotImplementedException();
    }

    public void ConsumeCost(IReadOnlyCollection<Cost> costs)
    {
        throw new NotImplementedException();
    }

    public bool TryGetResource(string resourceName, out IResource resource)
    {
        throw new NotImplementedException();
    }

    
}
