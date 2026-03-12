using AbilitySystem.Core;
using UnityEngine;

namespace AbilitySystem.Effects
{
    [CreateAssetMenu(fileName = "DamageEffectDefinition", menuName = "Ability System/Effects/DamageEffectDefinition")]
    public class DamageEffectDefinition : AbilityEffectDefinition
    {
        public float damageAmount;

        public override IAbilityEffect CreateEffect(IResourceBearer source)
        {
            return new DamageEffect(damageAmount, source);
        }
    }
}
