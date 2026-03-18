
using AbilitySystem.Core;
using AbilitySystem.Targeting;


namespace AbilitySystem.Effects
{
    public class DamageEffect : IAbilityEffect
    {
        private float _damageAmount;
        private readonly ICaster _source;

        public DamageEffect(float damageAmount, ICaster source)
        {
            _damageAmount = damageAmount;
            _source = source;
        }
        public AbilityEffectApplyResult ApplyTo(IAbilityTarget target)
        {
            if(target is IDamageable damageable)
            {
                damageable.TakeDamage(_damageAmount, _source);
                return AbilityEffectApplyResult.Applied;
            }

            return AbilityEffectApplyResult.SkippedUnsupportedTarget;
        }
    }
}