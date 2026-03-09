using UnityEngine;
using AbilitySystem.Core;

namespace AbilitySystem.Effects
{
    public abstract class AbilityEffectDefinition : ScriptableObject
    {
        public abstract IAbilityEffect CreateEffect(ICaster source);
    }
}
