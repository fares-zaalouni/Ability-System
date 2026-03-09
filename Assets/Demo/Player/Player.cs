using System.Collections.Generic;
using System.Linq;
using AbilitySystem.Core;
using AbilitySystem.Resources;
using AbilitySystem.Effects;
using AbilitySystem.Targeting;
using UnityEngine;
using AbilitySystem;

public class Player : MonoBehaviour, ICaster, IAbilityTarget
{
    [SerializeField] private List<ResourceDefinition> _resourceDefinitions = new List<ResourceDefinition>();
    [SerializeField] private List<LabeledAbility> _abilityDefinitions = new List<LabeledAbility>();
    private Dictionary<string, IResource> _resources = new Dictionary<string, IResource>();
    private Dictionary<string, AbilityDefinition> _abilities = new Dictionary<string, AbilityDefinition>();
    bool _casted = false;

    void Awake()
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
        foreach (var labeledAbility in _abilityDefinitions)
        {
            _abilities.Add(labeledAbility.Label, labeledAbility.Definition);
        }
    }

    void Update()
    {
        if (!_casted)
        {

            Debug.Log("Attempting to cast Fireball");
            if (_abilities.TryGetValue("fireball", out var abilityDef))
            {
                var abilityInstance = new AbilityInstance(abilityDef, this);
                abilityInstance.Cast();
            }

            _casted = true;
        }
    }

    public bool CanConsumeCost(Cost cost)
    {
        if (_resources.TryGetValue(cost.resourceName, out var resource))
        {
            return resource.CanConsume(cost.amount);
        }
        Debug.Log("Resource not found: " + cost.resourceName);
        return false;
    }

    public bool CanConsumeCost(IReadOnlyCollection<Cost> costs)
    {
        return costs.All(cost => CanConsumeCost(cost));
    }

    public void ConsumeCost(Cost cost)
    {
        if (CanConsumeCost(cost))
        {
            _resources[cost.resourceName].Consume(cost.amount);
        }
    }

    public void ConsumeCost(IReadOnlyCollection<Cost> costs)
    {
        Debug.Log($"Attempting to consume costs for ability:");
        Debug.Log($"Resource before:");
        foreach (var resource in _resources.Values)
        {
            Debug.Log($"- {resource.Name}: {resource.MaxAmount}");
        }
        if (CanConsumeCost(costs))
        {
            foreach (var cost in costs)
            {
                Debug.Log($"Consuming {cost.amount} of {cost.resourceName}");
                ConsumeCost(cost);
            }
        }
        Debug.Log($"Finished consuming costs. Current Resources:");
        foreach (var resource in _resources.Values)
        {
            Debug.Log($"- {resource.Name}: {resource.MaxAmount}");
        }
    }

    public bool IsTargetable()
    {
        return true;
    }

    public bool CanApplyEffect(IAbilityEffect effect)
    {
        throw new System.NotImplementedException();
    }

    public void ApplyEffect(IAbilityEffect effect)
    {
        throw new System.NotImplementedException();
    }

    public bool TryGetResource(string resourceName, out IResource resource)
    {
        Debug.Log($"Trying to get resource: {resourceName}");
        return _resources.TryGetValue(resourceName, out resource);
    }
}