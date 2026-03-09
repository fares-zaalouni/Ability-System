using UnityEngine;

namespace AbilitySystem.Core
{
    public abstract class AbilityActionDefinition : ScriptableObject
    {
        public abstract IAbilityAction CreateRuntimeAction();
    }
}