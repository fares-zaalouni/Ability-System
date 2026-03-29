using UnityEngine;

namespace AbilitySystem.Attributes
{
    [CreateAssetMenu(fileName = "ConsumableAttribute", menuName = "Ability System/Attributes/Consumable Attribute")]
    public class ConsumableAttributeDefinition : AttributeDefinition
    {
        [SerializeField] private float _initialAmount;
        public ConsumableAttribute CreateRuntimeConsumableAttribute()
        {
            return new ConsumableAttribute(_attributeName, _baseValue, _initialAmount);
        }

        public override Attribute CreateRuntimeAttribute()
        {
            return CreateRuntimeConsumableAttribute();
        }
    }
}