using AbilitySystem.Attributes;
using UnityEngine;

namespace AbilitySystem.Core
{
    enum ResourceValueType
    {
        Max,
        Current
    }
    [CreateAssetMenu(fileName = "AttributeThresholdConditionDefinition", menuName = "Ability System/Conditions/Attribute Threshold")]
    public class AttributeThresholdConditionDefinition : ConditionDefinition
    {
        [SerializeField] private AttributeDefinition _attributeDefinition;
        [SerializeField] private float _threshold;
        [SerializeField] private ResourceValueType _valueType = ResourceValueType.Current;

        public override bool Evaluate(AbilityContext context)
        {
            IAttributeBearer resourceBearer = context.Caster as IAttributeBearer;
            if (resourceBearer == null)
            {
                Debug.LogError("AttributeThresholdConditionDefinition can only be evaluated on casters that implement IAttibuteBearer.");
                return false;
            }

            var hasResource = resourceBearer.TryGetAttribute(_attributeDefinition.AttributeType, out var resource);

            if(!hasResource)
            {
                return false;
            }
            switch (_valueType)
            {
                case ResourceValueType.Max:
                    return resource != null && resource.CalculatedMaxValue >= _threshold;
                case ResourceValueType.Current:
                    return resource != null && resource.CalculatedValue >= _threshold;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(_valueType), _valueType, null);
            }
        }
    }
}