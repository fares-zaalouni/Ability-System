using UnityEngine;

public class DOTEffect : StatusEffect, IAbilityEffect
{
    public float damagePerTick;
    
    public DOTEffect( AbilityEffectDefinition definition, float damagePerTick, float duration, float tickInterval, ICaster source) 
    : base(definition, duration, tickInterval, 1, source)
    {
        this.damagePerTick = damagePerTick;
    }
    public void ApplyTo(IAbilityTarget target)
    {
        if (target is StatusEffectReceiver statusEffectReceiver)
        {
            statusEffectReceiver.ApplyStatusEffectTo(this, source);
        }
    }
    public override void ApplyTick(IAbilityTarget target)
    {
        if (target is IDamageable damageable)
        {
            damageable.TakeDamage(damagePerTick, source);
        }
    }
}