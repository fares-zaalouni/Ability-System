using UnityEngine;

namespace AbilitySystem.Resources
{
    public abstract class ResourceDefinition : ScriptableObject
    {
        [SerializeField] public string ResourceName;
        [SerializeField] protected float _maxAmount;
        public abstract IResource CreateRuntimeResource();
    }
}