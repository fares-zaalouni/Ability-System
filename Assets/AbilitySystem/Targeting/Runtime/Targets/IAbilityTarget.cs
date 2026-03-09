using System;

namespace AbilitySystem.Targeting.Targets
{
    public interface IAbilityTarget
    {
        bool IsTargetable();
        bool CanApplyEffect(IAbilityEffect effect);
    }
}
