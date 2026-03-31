using AbilitySystem.Targeting;

namespace AbilitySystem.Effects
{
    public interface IDurationPolicy
    {
        void HandleDuration(IAbilityTarget target, OverTimeEffect newEffect, OverTimeEffectGroup existingEffects);
    }
}