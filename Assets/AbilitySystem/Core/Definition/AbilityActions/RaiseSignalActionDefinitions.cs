using UnityEngine;

namespace AbilitySystem.Core
{
    [CreateAssetMenu(fileName = "RaiseSignalActionDefinition", menuName = "Ability System/Ability Actions/RaiseSignalActionDefinition")]
    public class RaiseSignalActionDefinition : AbilityActionDefinition
    {
        [SerializeField] private SignalDefinition _signal;

        public override IAbilityAction CreateRuntimeAction()
        {
            return new RaiseSignalAction(_signal);
        }
    }
}