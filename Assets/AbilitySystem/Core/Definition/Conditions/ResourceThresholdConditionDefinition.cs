using UnityEngine;

public class ResourceThresholdConditionDefinition : ConditionDefinition
{
    [SerializeField] private string _resourceName;
    [SerializeField] private float _threshold;

    public override bool Evaluate(AbilityContext context)
    {
        
    }
}