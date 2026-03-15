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
            IResourceBearer resourceBearer = context.Caster as IResourceBearer;
            if (resourceBearer == null)
            {
                Debug.LogError("ResourceThresholdConditionDefinition can only be evaluated on casters that implement IResourceBearer.");
                return false;
            }

            var hasResource = resourceBearer.TryGetResource(_resourceDefinition.ResourceName, out var resource);

            if(!hasResource)
            {
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