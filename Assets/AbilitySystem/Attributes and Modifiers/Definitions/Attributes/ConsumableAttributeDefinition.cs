using UnityEngine;

namespace AbilitySystem.Attributes
{
    [CreateAssetMenu(fileName = "ConsumableAttribute", menuName = "Ability System/Attributes/Consumable Attribute")]
    public class ConsumableAttributeDefinition : AttributeDefinition
    {
        public ConsumableAttribute CreateRuntimeConsumableAttribute()
        {
            return new ConsumableAttribute(_attributeName, _initialAmount);
        }

        public override Attribute CreateRuntimeAttribute()
        {
            return CreateRuntimeConsumableAttribute();
        }
    }
}