using System.Collections.Generic;
using UnityEngine;

public class AbilityContext
{
    public ICaster Caster { get; private set; }
    public List<IAbilityTarget> Targets { get; private set; }
    private readonly Dictionary<string, object> _blackboard = new();
    public AbilityContext(ICaster caster)
    {
        Caster = caster;
        Targets = new List<IAbilityTarget>();
    }
    public void Set<T>(string key, T value) => _blackboard[key] = value;

    public bool TryGet<T>(string key, out T value)
    {
        if (_blackboard.TryGetValue(key, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    
    public void SetTargets(List<IAbilityTarget> targets)
    {
        Targets = targets;
    }
    
}