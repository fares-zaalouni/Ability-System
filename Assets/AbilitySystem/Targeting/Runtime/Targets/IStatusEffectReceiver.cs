using System.Collections.Generic;

namespace AbilitySystem.Targeting.Targets
{
    public interface IStatusEffectReceiver
    {
        void ApplyStatusEffectTo(StatusEffect statusEffect, ICaster source = null, int stacks = 1);
    }
}
public interface IStatusEffectReceiver
{
    void ApplyStatusEffectTo(StatusEffect statusEffect, ICaster source = null, int stacks = 1);
}