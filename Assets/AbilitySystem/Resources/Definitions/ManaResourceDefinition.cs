using UnityEngine;

namespace AbilitySystem.Resources
{
    [CreateAssetMenu(fileName = "ManaResource", menuName = "Ability System/Resources/ManaResource")]
    public class ManaResourceDefinition : ResourceDefinition
    {
        [SerializeField] private float _regenAmount;
        public override IResource CreateRuntimeResource()
        {
            return new ManaResource(ResourceName, _maxAmount, _regenAmount);
        }
    }
}