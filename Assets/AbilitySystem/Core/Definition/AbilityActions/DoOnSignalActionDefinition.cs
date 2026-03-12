using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem.Core
{
    [CreateAssetMenu(fileName = "DoOnSignalActionDefinition", menuName = "Ability System/Ability Actions/DoOnSignalAction")]
    public class DoOnSignalActionDefinition : SustainedActionDefinition
    {
        [SerializeField] private SignalDefinition _triggerSignal;
        [SerializeField] private SignalDefinition _exitSignal;
        [SerializeField] private SustainedActionEndAftermath _subRunnerExitAftermath;
        [SerializeField] private List<AbilityActionDefinition> _subActions;
        public override IAbilityAction CreateRuntimeAction()
        {
            return new DoOnSignalAction(_triggerSignal, _exitSignal, _subActions.ConvertAll(a => a.CreateRuntimeAction()), _subRunnerExitAftermath, IsCancellable, IsInterruptible, CancelAfterMath, InterruptAfterMath);
        }
    }
}