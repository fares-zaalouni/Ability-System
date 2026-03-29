using AbilitySystem.Targeting;
using AbilitySystem.Core;
using AbilitySystem.Attributes;
using System.Collections.Generic;
using AbilitySystem.Utility;

namespace AbilitySystem.Effects
{
    public class DOTEffect : OverTimeEffect
    {
        private Attribute _damagePerTick;
        private List<IModifier> _modifiers;

        public DOTEffect(OverTimeEffectDefinition definition, float damagePerTick, float duration, float tickInterval,int initialStacks, int maxStacks, List<IModifier> modifiers, AbilityContext context)
        : base(definition, duration, tickInterval, initialStacks, maxStacks, context)
        {
            _damagePerTick = new Attribute(damagePerTick);
            _modifiers = modifiers;
        }
        public override void ApplyTickTo(IAbilityTarget target)
        {
            if (target is IDamageable damageable)
            {
                damageable.TakeDamage(_damagePerTick.RuntimeValue * Stacks, Context.Caster);
            }
        }

        public override void ApplyModifiers(IAbilityTarget target)
        {
            var targetAttributeHolder = target as IAttributeHolder;
            var sourceAttributeHolder = Context.Caster as IAttributeHolder;
            
            // Clear existing modifiers before reapplying
            _damagePerTick.ClearModifiers();
            
            foreach (var modifier in _modifiers)
            {
                if (modifier is AttributeModifier attributeModifier)
                {
                    if (attributeModifier.Source == ModifierSource.Caster)
                    {
                        AbilityDebug.Log($"Processing DOTEffect modifier from Caster. Attribute: {attributeModifier.AttributeName}, Percent: {attributeModifier.Percent:F4}");
                        if (sourceAttributeHolder == null)
                        {
                            AbilityDebug.LogError($"DOTEffect Modifier Error: Caster is not an attribute holder. Source: {Context.Caster}");
                            continue;
                        }
                        if (sourceAttributeHolder.TryGetAttribute(attributeModifier.AttributeName, out var sourceAttr))
                        {
                            attributeModifier.SetBonusAttribute(sourceAttr);
                            _damagePerTick.AddModifier(attributeModifier);
                        }
                        else AbilityDebug.LogError($"DOTEffect Modifier Error: Caster does not have the required attribute. Attribute Name: {attributeModifier.AttributeName}");
                    }

                    if (attributeModifier.Source == ModifierSource.Target)
                    {
                        if (targetAttributeHolder == null)
                        {
                            AbilityDebug.LogError($"DOTEffect Modifier Error: Target is not an attribute holder. Target: {target}");
                            continue;
                        }
                        if (targetAttributeHolder.TryGetAttribute(attributeModifier.AttributeName, out var targetAttr))
                        {
                            attributeModifier.SetBonusAttribute(targetAttr);
                            _damagePerTick.AddModifier(attributeModifier);
                        }
                        else AbilityDebug.LogError($"DOTEffect Modifier Error: Target does not have the required attribute. Attribute Name: {attributeModifier.AttributeName}");
                    }
                }
            }
        }
    }
}