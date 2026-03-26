using UnityEngine;

namespace AbilitySystem.Attributes
{
    public abstract class ModifierDefinition : ScriptableObject
    {
        [SerializeField] private int _priority;

        public abstract IModifier CreateRuntimeModifier();
    }
}