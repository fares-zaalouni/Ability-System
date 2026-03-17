using UnityEngine;

namespace AbilitySystem.Effects
{
    [CreateAssetMenu(fileName = "BasicStackingPolicyDefinition", menuName = "Ability System/Effects/Stacking Policies/Basic Stacking Policy")]
    public class BasicStackingPolicyDefinition : StackingPolicyDefinition
    {
        public override IStackingPolicy CreateRuntimeStackingStrategy()
        {
            return new BasicStackingPolicy(_durationRefreshPolicy, _stackingBehavior, _stackIfSameSource, _newInstance);
        }
    }
}