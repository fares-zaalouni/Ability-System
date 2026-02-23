using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffectDefinition", menuName = "Ability System/Effects/DamageEffectDefinition")]
public class DamageEffectDefinition : AbilityEffectDefinition
{
    public float damageAmount;

    public override IAbilityEffect CreateEffect(ICaster source)
    {
        return new DamageEffect(damageAmount, source);
    }
}
