using System;
using AbilitySystem.Targeting;

namespace AbilitySystem.Effects
{
    public enum AbilityEffectApplyResult
    {
        Applied,
        SkippedUnsupportedTarget,
        Failed
    }

    public interface IAbilityEffect
    {
        AbilityEffectApplyResult ApplyTo(IAbilityTarget target);
    }
}