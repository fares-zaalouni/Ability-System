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

        private ModifierSource _source;
        public ModifierSource Source => _source;

        private string _attributeName;
        public string AttributeName => _attributeName;

        private Attribute _bonusAttribute;
        private ModifierApplicationStrategy _applicationStrategy;

        public AttributeModifier(int priority, ModifierApplicationStrategy applicationStrategy, ModifierSource source, string attributeName, float percent)
        {
            _priority = priority;
            _applicationStrategy = applicationStrategy;
            _source = source;
            _percent = percent;
            _attributeName = attributeName;
        }

        public void SetBonusAttribute(Attribute attribute)
        {
            _bonusAttribute = attribute;
        }
        public void Apply(Attribute attribute)
        {
            switch (_applicationStrategy)
            {
                case ModifierApplicationStrategy.Base:
                    var baseBonus = _bonusAttribute.BaseValue * Percent;
                    attribute.AddToRuntime(baseBonus);
                    if(baseBonus != 0)
                        attribute.OnRuntimeValueChangedInvoke();

                    break;
                case ModifierApplicationStrategy.Runtime:
                    var runtimeBonus = _bonusAttribute.RuntimeValue * Percent;
                    attribute.AddToRuntime(runtimeBonus);
                    if(runtimeBonus != 0)
                        attribute.OnRuntimeValueChangedInvoke();
                    break;
                case ModifierApplicationStrategy.Current:
                    if (_bonusAttribute is IConsumableAttribute consumableBonus)
                    {
                        var currentBonus = consumableBonus.CurrentAmount * Percent;
                        attribute.AddToRuntime(currentBonus);
                        if(currentBonus != 0)
                            attribute.OnRuntimeValueChangedInvoke();
                    }
                    else AbilityDebug.LogError($"Trying to apply a Current Modifier with a non consumable bonus attribute. Attribute Name: {_bonusAttribute.Name}");
                    break;
            }

        }
    }
}