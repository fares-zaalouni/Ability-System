

using AbilitySystem.Core;

namespace AbilitySystem.Targeting
{
    public interface IDamageable
    {
        void TakeDamage(float amount, ICaster source = null);
    }
}
