
using AbilitySystem.Core;
using AbilitySystem.Targeting;
using AbilitySystem.Attributes;
using System.Collections.Generic;

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
                ModifierResolutionHelper.ResolveAndApplyModifiers(_modifiers, _damageAmount, _context, target, "DamageEffect");
                damageable.TakeDamage(_damageAmount.RuntimeValue, _context.Caster);
                return AbilityEffectApplyResult.Applied;
            }

            return AbilityEffectApplyResult.SkippedUnsupportedTarget;
        }
    }
}