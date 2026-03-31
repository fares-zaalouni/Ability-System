using UnityEngine;

namespace AbilitySystem.Effects
{
    [CreateAssetMenu(fileName = "BasicStackingPolicyDefinition", menuName = "Ability System/Effects/Stacking Policies/Basic Stacking Policy")]
    public class BasicStackingPolicyDefinition : StackingPolicyDefinition
    {
        public override IStackingPolicy CreateRuntimeStackingPolicy()
        {
            return new BasicStackingPolicy(_stackingBehavior, _stackIfSameSource, _newInstance);
        }
    }
}