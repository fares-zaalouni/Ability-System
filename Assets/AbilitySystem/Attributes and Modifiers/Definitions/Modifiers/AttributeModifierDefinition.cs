using UnityEngine;

namespace AbilitySystem.Attributes
{
    [CreateAssetMenu(fileName = "AttributeModifier", menuName = "Ability System/Attributes/Modifiers/Attribute Modifier")]
    public class AttributeModifierDefinition : ModifierDefinition
    {
        [SerializeField] private ModifierApplicationStrategy _applicationStrategy;
        [SerializeField] private ModifierSource _source;
        [SerializeField] private string _attributeName;
        public string AttributeName => _attributeName;
        [SerializeField] [Tooltip("Percent %")] private float _percent;

        public override IModifier CreateRuntimeModifier()
        {
            return new AttributeModifier(_priority, _applicationStrategy, _source, _attributeName, _percent * 0.01f);
        }
    }
}