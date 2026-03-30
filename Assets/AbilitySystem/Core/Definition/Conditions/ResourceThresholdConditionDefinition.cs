using AbilitySystem.Attributes;
using AbilitySystem.Utility;
using UnityEngine;

namespace AbilitySystem.Core
{
    enum ResourceValueType
    {
        Base,
        Runtime
    }
    [CreateAssetMenu(fileName = "AttributeThresholdConditionDefinition", menuName = "Ability System/Conditions/Attribute Threshold")]
    public class AttributeThresholdConditionDefinition : ConditionDefinition
    {
        [SerializeField] private AttributeDefinition _attributeDefinition;
        [SerializeField] private float _threshold;
        [SerializeField] private ResourceValueType _valueType = ResourceValueType.Runtime;

        public override bool Evaluate(AbilityContext context)
        {
            IAttributeHolder resourceBearer = context.Caster as IAttributeHolder;
            if (resourceBearer == null)
            {
                AbilityDebug.LogError("AttributeThresholdConditionDefinition can only be evaluated on casters that implement IAttibuteBearer.");
                return false;
            }

            var hasResource = resourceBearer.TryGetAttribute(_attributeDefinition.AttributeType, out var resource);

            if(!hasResource)
            {
                return false;
            }
            switch (_valueType)
            {
                case ResourceValueType.Base:
                    return resource != null && resource.BaseValue >= _threshold;
                case ResourceValueType.Runtime:
                    return resource != null && resource.RuntimeValue >= _threshold;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(_valueType), _valueType, null);
            }
        }
    }
}