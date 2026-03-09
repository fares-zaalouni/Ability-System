using UnityEngine;

namespace AbilitySystem.Targeting.Definition
{
    public abstract class TargetingStrategyDefinition : ScriptableObject
    {
        public abstract ITargetingStrategy CreateRuntimeStrategy();
    }
}
