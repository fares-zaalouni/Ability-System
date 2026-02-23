using UnityEngine;

[CreateAssetMenu(fileName = "DOTEffectDefinition", menuName = "Ability System/Effects/DOTEffectDefinition")]
public class DOTEffectDefinition : AbilityEffectDefinition
{
    public float damageAmount;
    public float duration;
    public float tickInterval;

    public override IAbilityEffect CreateEffect(ICaster source)
    {
        return new DOTEffect(this, damageAmount, duration, tickInterval, source);
    }
}
