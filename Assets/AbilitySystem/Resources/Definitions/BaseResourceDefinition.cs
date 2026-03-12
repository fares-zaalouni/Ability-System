using UnityEngine;

namespace AbilitySystem.Resources
{
    [CreateAssetMenu(fileName = "BaseResource", menuName = "Ability System/Resources/BaseResource")]
    public class BaseResourceDefinition : ResourceDefinition
    {
        public override IResource CreateRuntimeResource()
        {
            return new BaseResource(ResourceName, _maxAmount, _regenAmount);
        }
    }
}