using UnityEngine;
using AbilitySystem.Core;

namespace AbilitySystem.Effects
{
    [CreateAssetMenu(fileName = "DOTEffectDefinition", menuName = "Ability System/Effects/DOTEffectDefinition")]
    public class DOTEffectDefinition : AbilityEffectDefinition
    {
        [SerializeField] private float _damageAmount;
        [SerializeField] private float _duration;
        [SerializeField] private float _tickInterval;
        [SerializeField] private bool _applyOnce = false;

        public override IAbilityEffect CreateEffect(ICaster source)
        {
            return new DOTEffect(this, _damageAmount, _duration, _tickInterval, _applyOnce, source);
        }
    }
}
