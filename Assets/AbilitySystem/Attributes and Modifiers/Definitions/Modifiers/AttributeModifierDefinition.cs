using UnityEngine;

namespace AbilitySystem.Attributes
{
    public class AttributeModifierDefinition : ModifierDefinition
    {
        [SerializeField] private ModifierApplicationStrategy _applicationStrategy;
        [SerializeField] private float _percent;

        public override IModifier CreateRuntimeModifier()
        {
            return new AttributeModifier(_applicationStrategy, _percent);
        }
    }
}