
using AbilitySystem.Core;
using AbilitySystem.Targeting;
using AbilitySystem.Attributes;
using System.Collections.Generic;
using AbilitySystem.Utility;
using System.Diagnostics;

namespace AbilitySystem.Effects
{
    public class DamageEffect : IAbilityEffect
    {
        private Attribute _damageAmount;
        private List<IModifier> _modifiers;
        private readonly AbilityContext _context;

        public DamageEffect(float damageAmount, List<IModifier> modifiers, AbilityContext context)
        {

            _damageAmount = new Attribute(damageAmount);
            _context = context;
            _modifiers = modifiers;
        }
        public AbilityEffectApplyResult ApplyTo(IAbilityTarget target)
        {
            if(target is IDamageable damageable)
            {
                var targetAttributeHolder = target as IAttributeHolder;
                var sourceAttributeHolder = _context.Caster as IAttributeHolder;
                foreach (var modifier in _modifiers)
                {
                    if(modifier is AttributeModifier attributeModifier)
                    {
                        if(attributeModifier.Source == ModifierSource.Caster )
                        {
                            AbilityDebug.Log($"Processing modifier from Caster. Attribute: {attributeModifier.AttributeName}, Percent: {attributeModifier.Percent:F4}");
                            if(sourceAttributeHolder == null)
                            {
                                AbilityDebug.LogError($"DamageEffect Modifier Error: Caster is not an attribute holder. Source: {_context.Caster}");
                                continue;
                            }
                            if(sourceAttributeHolder.TryGetAttribute(attributeModifier.AttributeName, out var sourceAttr))
                            {
                                attributeModifier.SetBonusAttribute(sourceAttr);
                                _damageAmount.AddModifier(attributeModifier);
                            }
                            else AbilityDebug.LogError($"DamageEffect Modifier Error: Caster does not have the required attribute. Attribute Name: {attributeModifier.AttributeName}");
                        } 
                        
                        if(attributeModifier.Source == ModifierSource.Target)
                        {
                            if(targetAttributeHolder == null)
                            {
                                AbilityDebug.LogError($"DamageEffect Modifier Error: Target is not an attribute holder. Target: {target}");
                                continue;
                            }
                            if(targetAttributeHolder.TryGetAttribute(attributeModifier.AttributeName, out var targetAttr))
                            {
                                attributeModifier.SetBonusAttribute(targetAttr);
                                _damageAmount.AddModifier(attributeModifier);
                            }
                            else AbilityDebug.LogError($"DamageEffect Modifier Error: Target does not have the required attribute. Attribute Name: {attributeModifier.AttributeName}");
                        }
                        
                    }
                }
                damageable.TakeDamage(_damageAmount.RuntimeValue, _context.Caster);
                return AbilityEffectApplyResult.Applied;
            }

            return AbilityEffectApplyResult.SkippedUnsupportedTarget;
        }
    }
}