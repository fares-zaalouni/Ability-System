using UnityEngine;

namespace AbilitySystem.Attributes
{
    public abstract class ModifierDefinition : ScriptableObject
    {
        [SerializeField] protected int _priority;

        public abstract IModifier CreateRuntimeModifier();
    }
}