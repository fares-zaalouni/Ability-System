using UnityEngine;

namespace AbilitySystem.Core
{
    public abstract class ConditionDefinition : ScriptableObject
    {
        public abstract bool Evaluate(AbilityContext context);
    }
}
