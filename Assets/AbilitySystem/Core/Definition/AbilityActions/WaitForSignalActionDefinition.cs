using AbilitySystem.Core;
using UnityEngine;

namespace AbilitySystem.Core
{
    [CreateAssetMenu(fileName = "WaitForSignalActionDefinition", menuName = "Ability System/Ability Actions/WaitForSignalActionDefinition")]
    public class WaitForSignalActionDefinition : SustainedActionDefinition
    {        
        [SerializeField] private SignalDefinition _signalToWaitFor;

        public override IAbilityAction CreateRuntimeAction()
        {
            return new WaitForSignalAction(_signalToWaitFor, IsCancellable, IsInterruptible, CancelAfterMath, InterruptAfterMath);
        }
    }
}