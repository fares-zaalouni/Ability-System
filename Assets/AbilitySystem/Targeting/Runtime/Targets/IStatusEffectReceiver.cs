using System.Collections.Generic;
using AbilitySystem.Effects;
using AbilitySystem.Core;

namespace AbilitySystem.Targeting
{
    public interface IStatusEffectReceiver
    {
        void ApplyStatusEffectTo(StatusEffect statusEffect, IResourceBearer source = null, int stacks = 1);
    }
}