using AbilitySystem.Targeting;
using AbilitySystem.Core;
using AbilitySystem.Attributes;
using System.Collections.Generic;

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
            // Clear existing modifiers before reapplying
            _damagePerTick.ClearModifiers();

            ModifierResolutionHelper.ResolveAndApplyModifiers(_modifiers, _damagePerTick, Context, target, "DOTEffect");
        }
    }
}