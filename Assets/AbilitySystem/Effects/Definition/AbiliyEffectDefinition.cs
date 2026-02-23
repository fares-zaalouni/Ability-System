using UnityEngine;

public abstract class AbilityEffectDefinition : ScriptableObject
{
    public abstract IAbilityEffect CreateEffect(ICaster source);
}
