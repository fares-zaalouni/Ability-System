
using UnityEngine;

public abstract class AbilityActionDefinition : ScriptableObject
{
    public abstract IAbilityAction CreateRuntimeAction();
}