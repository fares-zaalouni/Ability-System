using UnityEngine;

namespace AbilitySystem.Resources
{
    public abstract class ResourceDefinition : ScriptableObject
    {
        [SerializeField] public string ResourceName;
        [SerializeField] protected float _maxAmount;
        [SerializeField] protected float _regenAmount;

        public abstract IResource CreateRuntimeResource();
    }
}