using System;
using UnityEngine;

public class Enemy : StatusEffectReceiver,
IAbilityTarget,
IDamageable

{
    private Health _health;

    void Start()
    {
        _health = GetComponent<Health>();
    }
    public bool IsTargetable()
    {
        return true;
    }
    public bool CanApplyEffect(IAbilityEffect effect)
    {
        if (effect is DamageEffect || effect is DOTEffect)
        {
            return true;
        }
        return false;
    }
    public void TakeDamage(float amount, ICaster source = null)
    {
        Debug.Log($"Enemy took {amount} damage from {source}");
        _health.TakeDamage(amount);
    }

   
}
