using UnityEngine;
using AbilitySystem.Targeting;

namespace AbilitySystem.Core
{
    [CreateAssetMenu(fileName = "TargetingAction", menuName = "Ability System/Ability Actions/TargetingAction")]
    public class TargetingActionDefinition : AbilityActionDefinition
    {
        [SerializeField] TargetingStrategyDefinition targetingStrategy;
        public override IAbilityAction CreateRuntimeAction()
        {
            return new TargetingAction(targetingStrategy.CreateRuntimeStrategy());
        }
    }
}
