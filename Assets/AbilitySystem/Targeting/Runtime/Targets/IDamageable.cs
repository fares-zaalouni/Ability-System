namespace AbilitySystem.Targeting.Targets
{
    public interface IDamageable
    {
        void TakeDamage(float amount, ICaster source = null);
    }
}