using System;
using AbilitySystem.Targeting;

namespace AbilitySystem.Effects
{
    public interface IAbilityEffect
    {
        void ApplyTo(IAbilityTarget target);
    }
}