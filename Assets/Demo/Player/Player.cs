using System.Collections.Generic;
using System.Linq;
using AbilitySystem.Core;
using AbilitySystem.Targeting;
using UnityEngine;
using System;
using AbilitySystem.Attributes;
using AbilitySystem.Utility;

using Attribute = AbilitySystem.Attributes.Attribute;
using AbilitySystem.Effects;
public class Player : MonoBehaviour,
ICaster,
IAttributeHolder,
IAbilityTarget
{
    [SerializeField] private ConsumableAttributeDefinition _healthAttributeDefinition;
    [SerializeField] private ConsumableAttributeDefinition _manaAttributeDefinition;
    [SerializeField] private AttributeDefinition _abilityPowerAttributeDefinition;
    private Dictionary<string, ConsumableAttribute> _consumableAttributes = new Dictionary<string, ConsumableAttribute>();
    private Dictionary<string, Attribute> _attributes = new Dictionary<string, Attribute>();
    [SerializeField] private List<AbilityDefinition> _abilityDefinitions = new List<AbilityDefinition>();

    private Dictionary<string, AbilityInstance> _abilities = new Dictionary<string, AbilityInstance>();
    bool _casted = false;
    private Dictionary<AbilityDefinition, Action<AbilityContext>[]> _castCompleteCallbacks = new Dictionary<AbilityDefinition, Action<AbilityContext>[]>();
    WeakReference<AbilityCast> _currentCast;

    void Awake()
    {
        RegisterAttributes();
        foreach (var abilityDef in _abilityDefinitions)
        {
            GrantAbility(abilityDef);
        }
    }
    public void RegisterAttributes()
    {
        var health = _healthAttributeDefinition.CreateRuntimeConsumableAttribute();
        var mana = _manaAttributeDefinition.CreateRuntimeConsumableAttribute();
        var abilityPower = _abilityPowerAttributeDefinition.CreateRuntimeAttribute();
        _consumableAttributes.Add(health.Name, health);
        _consumableAttributes.Add(mana.Name, mana);
        _attributes.Add(abilityPower.Name, abilityPower);
        AbilityDebug.Log($"Initialized resource: {health.Name} with MaxAmount: {health.RuntimeValue} and CurrentAmount: {health.CurrentAmount}");
        AbilityDebug.Log($"Initialized resource: {mana.Name} with MaxAmount: {mana.RuntimeValue} and CurrentAmount: {mana.CurrentAmount}");
        AbilityDebug.Log($"Initialized attribute: {abilityPower.Name} with Value: {abilityPower.RuntimeValue}");
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
                if (_currentCast.TryGetTarget(out var cast))
                {
                    cast.OnCompleted += (ctx) => AbilityDebug.Log("Fireball cast completed!");
                    cast.OnCancelled += (ctx) => AbilityDebug.Log("Fireball cast cancelled!");
                    cast.OnInterrupted += (ctx) => AbilityDebug.Log("Fireball cast interrupted!");
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.F))
        {

            AbilityDebug.Log("Attempting to Interrupt Fireball");

            if (_currentCast.TryGetTarget(out var cast) && _currentCast != null)
            {
                cast?.Interrupt();
            }

        }
    }

    public bool CanConsumeCost(Attribute cost)
    {
        if (_consumableAttributes.TryGetValue(cost.Name, out var consumableAttribute))
        {
            return consumableAttribute.CanConsume(cost.RuntimeValue);
        }
        AbilityDebug.Log("Resource not found: " + cost.Name);
        return false;
    }

    public bool CanConsumeCost(IReadOnlyCollection<Attribute> costs)
    {
        return costs.All(cost => CanConsumeCost(cost));
    }

    public void ConsumeCost(Attribute cost)
    {
        if (CanConsumeCost(cost))
        {
            if (_consumableAttributes.TryGetValue(cost.Name, out var consumableAttribute))
            {
                AbilityDebug.Log($"Consuming {cost.RuntimeValue} of {consumableAttribute.Name}");
                consumableAttribute.Consume(cost.RuntimeValue);
            }
        }
    }

    public void ConsumeCost(IReadOnlyCollection<Attribute> costs)
    {
        AbilityDebug.Log($"Attempting to consume costs for ability:");
        AbilityDebug.Log($"Resource before:");
        foreach (var resource in _consumableAttributes.Values)
        {
            AbilityDebug.Log($"- {resource.Name}: {(resource is IConsumableAttribute consumable ? consumable.CurrentAmount : resource.RuntimeValue)}");
        }
        if (CanConsumeCost(costs))
        {
            foreach (var cost in costs)
            {
                ConsumeCost(cost);
            }
        }
        AbilityDebug.Log($"Finished consuming costs. Current Resources:");
        foreach (var resource in _consumableAttributes.Values)
        {
            AbilityDebug.Log($"- {resource.Name}: {(resource is IConsumableAttribute consumable ? consumable.CurrentAmount : resource.RuntimeValue)}");
        }
    }

    public bool IsTargetable()
    {
        return true;
    }

    public bool TryGetAttribute(string attributeName, out Attribute attribute)
    {
        AbilityDebug.Log($"Trying to get attribute: {attributeName}");
        if (_consumableAttributes.TryGetValue(attributeName, out var consumableAttribute))
        {
            attribute = consumableAttribute;
            return true;
        }
        return _attributes.TryGetValue(attributeName, out attribute);
    }

    public void GrantAbility(AbilityDefinition abilityDefinition)
    {
        if (_abilities.ContainsKey(abilityDefinition.AbilityName))
        {
            AbilityDebug.LogWarning($"Ability {abilityDefinition.AbilityName} already granted to player.");
            return;
        }
        var abilityInstance = new AbilityInstance(abilityDefinition, this, this);
        var callbacks = new Action<AbilityContext>[3];
        callbacks[0] = (ctx) => AbilityDebug.Log($"{abilityDefinition.AbilityName} cast completed!");
        callbacks[1] = (ctx) => AbilityDebug.Log($"{abilityDefinition.AbilityName} cast cancelled!");
        callbacks[2] = (ctx) => AbilityDebug.Log($"{abilityDefinition.AbilityName} cast interrupted!");
        _castCompleteCallbacks[abilityDefinition] = callbacks;
        _abilities.Add(abilityDefinition.AbilityName, abilityInstance);
        SignalBus.Subscribe(abilityDefinition.CastCompleteSignal, callbacks[0]);
        SignalBus.Subscribe(abilityDefinition.CastCancelSignal, callbacks[1]);
        SignalBus.Subscribe(abilityDefinition.CastInterruptSignal, callbacks[2]);
        CooldownManager.Instance.RegisterCooldown(this, abilityInstance.Id, abilityInstance.Cooldown);
        AbilityDebug.Log($"Granted ability: {abilityDefinition.AbilityName}");
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
            AbilityDebug.Log($"Removed ability: {abilityDefinition.AbilityName}");
        }
        else
        {
            AbilityDebug.LogWarning($"Ability {abilityDefinition.AbilityName} not found.");
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