using UnityEngine;

namespace AbilitySystem.Effects
{
    public abstract class DurationPolicyDefinition : ScriptableObject
    {
        public abstract IDurationPolicy CreateRuntimeDurationPolicy();
    }
}