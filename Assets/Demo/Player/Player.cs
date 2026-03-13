using System.Collections.Generic;
using System.Linq;
using AbilitySystem.Core;
using AbilitySystem.Resources;
using AbilitySystem.Effects;
using AbilitySystem.Targeting;
using UnityEngine;
using AbilitySystem;
using System;

public class Player : MonoBehaviour, 
ICaster,
IResourceBearer, 
IAbilityTarget
{
    [SerializeField] private List<ResourceDefinition> _resourceDefinitions = new List<ResourceDefinition>();
    [SerializeField] private List<AbilityDefinition> _abilityDefinitions = new List<AbilityDefinition>();
    private Dictionary<string, IResource> _resources = new Dictionary<string, IResource>();
    private Dictionary<string, AbilityInstance> _abilities = new Dictionary<string, AbilityInstance>();
    bool _casted = false;
    WeakReference<AbilityCast> _currentCast;

    void Awake()
    {
        RegisterResources();
        foreach (var abilityDef in _abilityDefinitions)
        {
            GrantAbility(abilityDef);
        }
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_abilities.TryGetValue("fireball", out var abilityInstance))
            {
                Blackboard initialBlackboard = new Blackboard
                {
                    { ContextKeys.ProjectileLaunchDirection, transform.forward },
                    { ContextKeys.ProjectileSpawnPoint, transform.position + transform.forward * 1.5f }
                };
                abilityInstance.Cast(out _currentCast, initialBlackboard);
                if(_currentCast.TryGetTarget(out var cast))
                {
                    cast.OnCompleted += (ctx) => Debug.Log("Fireball cast completed!");
                    cast.OnCancelled += (ctx) => Debug.Log("Fireball cast cancelled!");
                    cast.OnInterrupted += (ctx) => Debug.Log("Fireball cast interrupted!");
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.F))
        {

            Debug.Log("Attempting to Interrupt Fireball");
            
            if(_currentCast.TryGetTarget(out var cast) && _currentCast != null)
            {
                cast?.Interrupt();
            }
            
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

    public void GrantAbility(AbilityDefinition abilityDefinition)
    {
        if (_abilities.ContainsKey(abilityDefinition.AbilityName))
        {
            Debug.LogWarning($"Ability {abilityDefinition.AbilityName} already granted to player.");
            return;
        }
        var abilityInstance = new AbilityInstance(abilityDefinition, this);
         _abilities.Add(abilityDefinition.AbilityName, abilityInstance);
            SignalBus.Subscribe(abilityDefinition.CastCompleteSignal, 
                (ctx) => Debug.Log($"Received cast complete signal for {abilityDefinition.AbilityName}"));
            SignalBus.Subscribe(abilityDefinition.CastCancelSignal, 
                (ctx) => Debug.Log($"Received cast cancel signal for {abilityDefinition.AbilityName}"));
            SignalBus.Subscribe(abilityDefinition.CastInterruptSignal, 
                (ctx) => Debug.Log($"Received cast interrupt signal for {abilityDefinition.AbilityName}"));
        _abilities.Add(abilityDefinition.AbilityName, abilityInstance);
        CooldownManager.Instance.RegisterCooldown(this, abilityInstance.Id, abilityInstance.Cooldown);
        Debug.Log($"Granted ability: {abilityDefinition.AbilityName}");
    }

    public void RemoveAbility(AbilityDefinition abilityDefinition)
    {
        if (_abilities.ContainsKey(abilityDefinition.AbilityName))
        {
            _abilities.Remove(abilityDefinition.AbilityName);
            Debug.Log($"Removed ability: {abilityDefinition.AbilityName}");
        }
        else
        {
            Debug.LogWarning($"Ability {abilityDefinition.AbilityName} not found.");
        }
    }
}