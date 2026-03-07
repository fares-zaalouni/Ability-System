
using System;

public interface IAbilityTarget
{
    bool IsTargetable();
    bool CanApplyEffect(IAbilityEffect effect);
}
