using UnityEngine;
using AbilitySystem.Targeting;
using AbilitySystem.Core;
using System.Collections.Generic;

namespace AbilitySystem.Effects
{
    public class DOTEffect : OverTimeEffect
    {
        public float damagePerTick;

        public DOTEffect(AbilityEffectDefinition definition, float damagePerTick, float duration, float tickInterval, bool applyOnce, ICaster source)
        : base(definition, duration, tickInterval, applyOnce, 1, source)
        {
            this.damagePerTick = damagePerTick;
        }
        public override void ApplyTo(IAbilityTarget target)
        {
            if (target is IDamageable damageable)
            {
                damageable.TakeDamage(damagePerTick * Stacks, _source);
            }
        }

        public override void HandleStacking(IAbilityTarget target, List<OverTimeEffect> existingEffects)
        {
            if (existingEffects.Count > 0)
            {
                OverTimeEffect currentEffect = existingEffects[0];
                if (currentEffect.Stacks < currentEffect.MaxStacks)
                {
                    currentEffect.AddStacks(1);
                }
                currentEffect.RefreshDuration();
            }
            else
                existingEffects.Add(this);
        }
        public override void HandleExpiration(IAbilityTarget target, List<OverTimeEffect> existingEffects)
        {
            var currentEffect = existingEffects.Find(e => e.Id == Id);
            if (currentEffect != null)
            {
                currentEffect.RemoveStacks(1);
                if (currentEffect.Stacks <= 0)
                {
                    existingEffects.Remove(currentEffect);
                }
            }
            else
            {
                Debug.LogError($"Attempting to expire an effect that is not active on target {target}.");
            }
        }
    }
}