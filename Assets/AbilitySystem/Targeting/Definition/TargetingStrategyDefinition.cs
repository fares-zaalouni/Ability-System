using UnityEngine;

public abstract class TargetingStrategyDefinition : ScriptableObject
{
    public abstract ITargetingStrategy CreateRuntimeStrategy();
}
