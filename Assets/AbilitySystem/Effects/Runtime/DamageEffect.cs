
using AbilitySystem.Core;
using AbilitySystem.Targeting;


namespace AbilitySystem.Effects
{
    public class DamageEffect : IAbilityEffect
    {
        private float _damageAmount;
        private readonly IResourceBearer _source;

        public DamageEffect(float damageAmount, IResourceBearer source)
        {
            _damageAmount = damageAmount;
            _source = source;
        }
        public void ApplyTo(IAbilityTarget target)
        {
            if(target is IDamageable damageable)
            {
                damageable.TakeDamage(_damageAmount, _source);
            }
        }
    }
}