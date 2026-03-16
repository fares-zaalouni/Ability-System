using UnityEngine;
using AbilitySystem.Targeting;
using AbilitySystem.Core;
using System.Collections.Generic;

namespace AbilitySystem.Effects
{
    public class DOTEffect : OverTimeEffect
    {
        public float damagePerTick;

        public DOTEffect(OverTimeEffectDefinition definition, float damagePerTick, float duration, float tickInterval,int initialStacks, int maxStacks, ICaster source)
        : base(definition, duration, tickInterval, initialStacks, maxStacks, source)
        {
            this.damagePerTick = damagePerTick;
        }
        public override void ApplyTickTo(IAbilityTarget target)
        {
            if (target is IDamageable damageable)
            {
                damageable.TakeDamage(damagePerTick * Stacks, Source);
            }
        }
    }
}