using UnityEngine;

namespace AbilitySystem.Attributes
{
    [CreateAssetMenu(fileName = "ConsumableAttribute", menuName = "Ability System/Attributes/Consumable Attribute")]
    public class ConsumableAttributeDefinition : AttributeDefinition
    {
        public override string AttributeType => GetInstanceID().ToString();
        public override Attribute CreateRuntimeResource()
        {
            return new ConsumableAttribute(_attributeName, _initialAmount, _maxAmount);
        }
    }
}