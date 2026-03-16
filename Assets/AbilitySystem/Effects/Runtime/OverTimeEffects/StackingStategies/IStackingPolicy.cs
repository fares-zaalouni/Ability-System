using AbilitySystem.Targeting;

namespace AbilitySystem.Effects
{
    public interface IStackingPolicy
    {
        /*
            * Handles the stacking logic when a new effect is applied to a target that already has an existing effect of the same type.
            * @param target The target to which the effect is being applied.
            * @param newEffect The new effect that is being applied.
            * @param existingEffects The group of existing effects of the same type on the target.
            * @return Returns true if the new effect should be added to the existing group, or false if it should not be added (e.g., if it was merged or rejected).
        */
        bool HandleStacking(IAbilityTarget target, OverTimeEffect newEffect, OverTimeEffectGroup existingEffects);
    }
}