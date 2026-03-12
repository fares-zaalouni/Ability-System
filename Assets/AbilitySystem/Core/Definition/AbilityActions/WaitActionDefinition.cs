using UnityEngine;

namespace AbilitySystem.Core
{
    [CreateAssetMenu(fileName = "WaitAction", menuName = "Ability System/Ability Actions/WaitAction")]
    public class WaitActionDefinition : SustainedActionDefinition
    {
        public float Duration;

        public override IAbilityAction CreateRuntimeAction()
        {
            return new WaitAction(IsCancellable, IsInterruptible, CancelAfterMath, InterruptAfterMath, Duration);
        }
    }
}