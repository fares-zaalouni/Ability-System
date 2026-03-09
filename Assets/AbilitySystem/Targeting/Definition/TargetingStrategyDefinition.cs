using UnityEngine;

namespace AbilitySystem.Targeting
{
    public abstract class TargetingStrategyDefinition : ScriptableObject
    {
        public abstract ITargetingStrategy CreateRuntimeStrategy();
    }
}
