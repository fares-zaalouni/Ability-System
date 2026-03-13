using System;

namespace AbilitySystem.Core
{
    public interface ICooldownManager
    {
        void StartCooldown(ICaster caster, Guid cooldownId);
        void TickCooldowns(float deltaTime);

    }   
}