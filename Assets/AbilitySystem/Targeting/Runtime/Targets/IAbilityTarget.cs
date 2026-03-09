using System;
using AbilitySystem.Effects;

namespace AbilitySystem.Targeting
{
    public interface IAbilityTarget
    {
        bool IsTargetable();
        bool CanApplyEffect(IAbilityEffect effect);
    }
}
