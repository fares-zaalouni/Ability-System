using UnityEngine;

public abstract class ConditionDefinition : ScriptableObject
{
    public abstract bool Evaluate(AbilityContext context);
}
