
using AbilitySystem.Core;
using AbilitySystem.Targeting;
using AbilitySystem.Attributes;

namespace AbilitySystem.Effects
{
    public class DamageEffect : IAbilityEffect
    {
        private Attribute _damageAmount;
        private readonly ICaster _source;

        public DamageEffect(float damageAmount, ICaster source)
        {
            _damageAmount = new Attribute(damageAmount, damageAmount);
            _source = source;
        }
        public AbilityEffectApplyResult ApplyTo(IAbilityTarget target)
        {
            if(target is IDamageable damageable)
            {
                damageable.TakeDamage(_damageAmount.CalculatedValue, _source);
                return AbilityEffectApplyResult.Applied;
            }

            return AbilityEffectApplyResult.SkippedUnsupportedTarget;
        }
    }
}