using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem.Core
{
    [CreateAssetMenu(fileName = "RepeatAction", menuName = "Ability System/Ability Actions/RepeatAction")]
    public class RepeatActionDefinition : SustainedActionDefinition
    {
        [SerializeField] private float _tickInterval;
        [SerializeField] private float _duration;
        [SerializeField] private List<AbilityActionDefinition> _actions;
        [SerializeField] private SubRunnerCleanupMode _subRunnerCleanupMode = SubRunnerCleanupMode.RespectChildAftermath;
        public override IAbilityAction CreateRuntimeAction()
        {
            return new RepeatAction(_tickInterval, _duration, _actions.ConvertAll(a => a.CreateRuntimeAction()), _subRunnerCleanupMode, IsCancellable, IsInterruptible, CancelAfterMath, InterruptAfterMath);
        }
    }
}