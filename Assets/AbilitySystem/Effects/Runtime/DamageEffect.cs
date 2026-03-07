using System;
using UnityEngine;

public class DamageEffect : IAbilityEffect
{
    public float DamageAmount;
    public readonly ICaster Source;


    public DamageEffect(float damageAmount, ICaster source)
    {
        this.DamageAmount = damageAmount;
        this.Source = source;
    }
    public void ApplyTo(IAbilityTarget target)
    {
        if(target is IDamageable damageable)
        {
            damageable.TakeDamage(DamageAmount, Source);
        }
       
    }
}