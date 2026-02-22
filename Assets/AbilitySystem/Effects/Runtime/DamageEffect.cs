public struct DamageEffect : IAbilityEffect
{
    public float DamageAmount;
    public readonly ICaster Source;
    public DamageEffect(float damageAmount, ICaster source)
    {
        this.DamageAmount = damageAmount;
        this.Source = source;
    }
}