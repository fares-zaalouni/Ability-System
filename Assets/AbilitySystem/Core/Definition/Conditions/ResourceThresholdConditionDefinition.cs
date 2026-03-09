using AbilitySystem.Resources;
using UnityEngine;

namespace AbilitySystem.Core
{
    enum ResourceValueType
    {
        Max,
        Current
    }
    [CreateAssetMenu(fileName = "ResourceThresholdConditionDefinition", menuName = "Ability System/Conditions/Resource Threshold")]
    public class ResourceThresholdConditionDefinition : ConditionDefinition
    {
        [SerializeField] private ResourceDefinition _resourceDefinition;
        [SerializeField] private float _threshold;
        [SerializeField] private ResourceValueType _valueType = ResourceValueType.Current;

        public override bool Evaluate(AbilityContext context)
        {
            var hasResource = context.Caster.TryGetResource(_resourceDefinition.ResourceName, out var resource);
            Debug.Log($"Evaluating ResourceThresholdCondition: Caster has resource '{_resourceDefinition.ResourceName}': {hasResource}");
            if(!hasResource)
            {
                Debug.Log($"Resource '{_resourceDefinition.ResourceName}' not found on caster.");
                return false;
            }
            switch (_valueType)
            {
                case ResourceValueType.Max:
                    return resource != null && resource.MaxAmount >= _threshold;
                case ResourceValueType.Current:
                    return resource != null && resource.CurrentAmount >= _threshold;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(_valueType), _valueType, null);
            }
        }
    }
}