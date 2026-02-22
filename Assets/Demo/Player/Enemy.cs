using System;
using UnityEngine;

public class Enemy : MonoBehaviour, IAbilityTarget
{
    public void ApplyEffect(IAbilityEffect effect)
    {
        Debug.Log($"Enemy received effect: {effect}");
        switch (effect)
        {
            case DamageEffect damageEffect:
                Debug.Log($"Enemy took {damageEffect.DamageAmount} damage from {damageEffect.Source}");
                break;
            default:
                Debug.Log("Enemy received an unknown effect");
                break;
        }
        
    }
    public bool IsTargetable()
    {
        return true;
    }
    public bool CanApplyEffect(IAbilityEffect effect)
    {
        if (effect is DamageEffect)
        {
            return true;
        }
        return false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
