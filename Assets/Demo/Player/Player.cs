using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour, ICaster, IAbilityTarget
{
    private Dictionary<string, IResource> _resources = new Dictionary<string, IResource>();
    private Dictionary<string, AbilityInstance> _abilities = new Dictionary<string, AbilityInstance>();
    [SerializeField] private FireBall _fireBallPrefab;
    private FireBall _fireBallInstance;
    bool casted = false;

    void Awake()
    {
        AddResource(new Mana(100f));
        _fireBallInstance = Instantiate(_fireBallPrefab);
        _fireBallInstance.Initialize(this);
    }

    void Update()
    {
        if(!casted)
        {
            Vector3 targetPoint = transform.position + transform.forward * 2f;
            _fireBallInstance.CastFireBall(targetPoint);
            casted = true;
        }
    }
    
    private void AddResource(IResource resource)
    {
        if (!_resources.ContainsKey(resource.ResourceName.ToLower()))
        {
            _resources.Add(resource.ResourceName.ToLower(), resource);
        }
    }

    public bool CanConsumeCost(Cost cost)
    {
        if (_resources.TryGetValue(cost.resourceName.ToLower(), out var resource))
        {
            return resource.CanConsume(cost.amount);
        }
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
            _resources[cost.resourceName.ToLower()].Consume(cost.amount);
        }
    }

    public void ConsumeCost(IReadOnlyCollection<Cost> costs)
    {
        Debug.Log($"Attempting to consume costs for ability:");
        Debug.Log($"Resource before:");
        foreach (var resource in _resources.Values)
        {
            Debug.Log($"- {resource.ResourceName}: {resource.ResourceAmount}");
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
            Debug.Log($"- {resource.ResourceName}: {resource.ResourceAmount}");
        }
    }
    public void AddAbility(AbilityDefinition ability)
    {
        AbilityInstance instance = new AbilityInstance(ability, this);
        _abilities.Add(ability.abilityName.ToLower(), instance);
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
}