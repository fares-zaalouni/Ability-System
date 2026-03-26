using System;
using AbilitySystem.Utility;

namespace AbilitySystem.Attributes
{
    public class AttributeModifier : IPercentModifier
    {
        private int _priority;
        public int Priority => _priority;
        private float _percent;
        public float Percent => _percent;

        private Attribute _bonusAttribute;
        private ModifierApplicationStrategy _applicationStrategy;

        public AttributeModifier(ModifierApplicationStrategy applicationStrategy, float percent)
        {
            _applicationStrategy = applicationStrategy;
            _percent = percent;
        }
        public void Apply(Attribute attribute)
        {

            switch (_applicationStrategy)
            {
                case ModifierApplicationStrategy.Base:
                    attribute.AddToRuntime(_bonusAttribute.BaseValue * Percent);
                    break;
                case ModifierApplicationStrategy.Runetime:
                    attribute.AddToRuntime(_bonusAttribute.RuntimeValue * Percent);
                    break;
                case ModifierApplicationStrategy.Current:
                    if (_bonusAttribute is IConsumableAttribute consumableBonus)
                    {
                        attribute.AddToRuntime(consumableBonus.CurrentAmount * Percent);
                    }
                    else AbilityDebug.LogError($"Trying to apply a Current Modifier with a non consumable bonus attribute. Attribute Name: {_bonusAttribute.Name}");
                    break;
            }

        }
    }
}