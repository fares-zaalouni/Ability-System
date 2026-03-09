using UnityEngine;

public class HasTargetConditionDefinition : ConditionDefinition
{
    [SerializeField] private int _minTargets = 1;

    public override bool Evaluate(AbilityContext context)
    {
        return context.Targets.Count > _minTargets;
    }
}