using System.Collections.Generic;
using System.Linq;
using AbilitySystem.Core;
using AbilitySystem.Targeting;
using UnityEngine;
using System;
using AbilitySystem.Attributes;

using Attribute = AbilitySystem.Attributes.Attribute;
public class Player : MonoBehaviour, 
ICaster,
IAttributeBearer, 
IAbilityTarget
{
    [SerializeField] private ConsumableAttributeDefinition _healthAttributeDefinition;
    [SerializeField] private ConsumableAttributeDefinition _manaAttributeDefinition;
    private Dictionary<string, Attribute> _attributes = new Dictionary<string, Attribute>();
    [SerializeField] private List<AbilityDefinition> _abilityDefinitions = new List<AbilityDefinition>();

    private Dictionary<string, AbilityInstance> _abilities = new Dictionary<string, AbilityInstance>();
    bool _casted = false;
    private Dictionary<AbilityDefinition, Action<AbilityContext>[]> _castCompleteCallbacks = new Dictionary<AbilityDefinition, Action<AbilityContext>[]>();
    WeakReference<AbilityCast> _currentCast;

    void Awake()
    {
        RegisterAttribute();
        foreach (var abilityDef in _abilityDefinitions)
        {
            GrantAbility(abilityDef);
        }
    }
    public void RegisterAttribute()
    {
        var healthResource = _healthAttributeDefinition.CreateRuntimeResource();
        var manaResource = _manaAttributeDefinition.CreateRuntimeResource();
        _attributes.Add(_healthAttributeDefinition.AttributeType, healthResource);
        _attributes.Add(_manaAttributeDefinition.AttributeType, manaResource);
        Debug.Log($"Initialized resource: {healthResource.Name} with MaxAmount: {healthResource.CalculatedMaxValue} and CurrentAmount: {healthResource.CalculatedValue}");
        Debug.Log($"Initialized resource: {manaResource.Name} with MaxAmount: {manaResource.CalculatedMaxValue} and CurrentAmount: {manaResource.CalculatedValue}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_abilities.TryGetValue("fireball", out var abilityInstance))
            {
                var initialDependencies = new List<object>
                {
                    new ProjectileLaunchDirection(transform.forward),
                    new ProjectileSpawnPoint(transform.position + transform.forward * 1.5f)
                };
                abilityInstance.Cast(out _currentCast, initialDependencies);
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

    public bool CanConsumeCost(AbilityCost cost)
    {
        if (_attributes.TryGetValue(cost.attributeName, out var resource) && resource is IConsumableAttribute consumableResource)
        {
            return consumableResource.CanConsume(cost.cost);
        }
        Debug.Log("Resource not found: " + cost.attributeName);
        return false;
    }

    public bool CanConsumeCost(IReadOnlyCollection<AbilityCost> costs)
    {
        return costs.All(cost => CanConsumeCost(cost));
    }

    public void ConsumeCost(AbilityCost cost)
    {
        if (CanConsumeCost(cost))
        {
            if (_attributes.TryGetValue(cost.attributeName, out var resource) && resource is IConsumableAttribute consumableResource)
            {
                Debug.Log($"Consuming {cost.cost} of {resource.Name}");
                consumableResource.Consume(cost.cost);
            }
        }
    }

    public void ConsumeCost(IReadOnlyCollection<AbilityCost> costs)
    {
        Debug.Log($"Attempting to consume costs for ability:");
        Debug.Log($"Resource before:");
        foreach (var resource in _attributes.Values)
        {
            Debug.Log($"- {resource.Name}: {resource.CalculatedMaxValue}");
        }
        if (CanConsumeCost(costs))
        {
            foreach (var cost in costs)
            {
                ConsumeCost(cost);
            }
        }
        Debug.Log($"Finished consuming costs. Current Resources:");
        foreach (var resource in _attributes.Values)
        {
            Debug.Log($"- {resource.Name}: {resource.CalculatedMaxValue}");
        }
    }

    public bool IsTargetable()
    {
        return true;
    }

    public bool TryGetAttribute(string resourceName, out Attribute attribute)
    {
        Debug.Log($"Trying to get resource: {resourceName}");
        return _attributes.TryGetValue(resourceName, out attribute);
    }

    public void GrantAbility(AbilityDefinition abilityDefinition)
    {
        if (_abilities.ContainsKey(abilityDefinition.AbilityName))
        {
            Debug.LogWarning($"Ability {abilityDefinition.AbilityName} already granted to player.");
            return;
        }
        var abilityInstance = new AbilityInstance(abilityDefinition, this, this);
        var callbacks = new Action<AbilityContext>[3];
        callbacks[0] = (ctx) => Debug.Log($"{abilityDefinition.AbilityName} cast completed!");
        callbacks[1] = (ctx) => Debug.Log($"{abilityDefinition.AbilityName} cast cancelled!");
        callbacks[2] = (ctx) => Debug.Log($"{abilityDefinition.AbilityName} cast interrupted!");
        _castCompleteCallbacks[abilityDefinition] = callbacks;
         _abilities.Add(abilityDefinition.AbilityName, abilityInstance);
            SignalBus.Subscribe(abilityDefinition.CastCompleteSignal, callbacks[0]);
            SignalBus.Subscribe(abilityDefinition.CastCancelSignal, callbacks[1]);
            SignalBus.Subscribe(abilityDefinition.CastInterruptSignal, callbacks[2]);
        CooldownManager.Instance.RegisterCooldown(this, abilityInstance.Id, abilityInstance.Cooldown);
        Debug.Log($"Granted ability: {abilityDefinition.AbilityName}");
    }

    public void RemoveAbility(AbilityDefinition abilityDefinition)
    {
        if (_abilities.ContainsKey(abilityDefinition.AbilityName))
        {
            var ability = _abilities[abilityDefinition.AbilityName];
            ability.Dispose();
            _abilities.Remove(abilityDefinition.AbilityName);
            UnSubscribeAbility(abilityDefinition);
            CooldownManager.Instance.UnregisterCooldown(this, ability.Id);
            Debug.Log($"Removed ability: {abilityDefinition.AbilityName}");
        }
        else
        {
            Debug.LogWarning($"Ability {abilityDefinition.AbilityName} not found.");
        }
    }
    private void UnSubscribeAbility(AbilityDefinition abilityDefinition)
    {
        if (_castCompleteCallbacks.TryGetValue(abilityDefinition, out var callbacks))
        {
            SignalBus.Unsubscribe(abilityDefinition.CastCompleteSignal, callbacks[0]);
            SignalBus.Unsubscribe(abilityDefinition.CastCancelSignal, callbacks[1]);
            SignalBus.Unsubscribe(abilityDefinition.CastInterruptSignal, callbacks[2]);
            _castCompleteCallbacks.Remove(abilityDefinition);
        }
    }
    private void Dispose()
    {
        foreach (var ability in _abilities.Values)
        {
            UnSubscribeAbility(ability.Definition);
            ability.Dispose();
        }
        _abilities.Clear();
        _castCompleteCallbacks.Clear();
    }

    public void OnDestroy()
    {
        Dispose();
    }
}