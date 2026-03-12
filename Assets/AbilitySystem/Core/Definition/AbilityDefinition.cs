using System.Collections.Generic;
using UnityEngine;
using AbilitySystem.Resources;
using AbilitySystem.Costs;

namespace AbilitySystem.Core
{
    [CreateAssetMenu(fileName = "AbilityDefinition", menuName = "Ability System/AbilityDefinition")]
    public class AbilityDefinition : ScriptableObject
    {
        [SerializeField] public string AbilityName;
        [SerializeField] public float Cooldown;
        [SerializeField] public List<AbilityCostDefinition> Costs;
        [SerializeField] public List<AbilityActionDefinition> ActionDefinitions;
        [SerializeField] public SignalDefinition CastCompleteSignal;
        [SerializeField] public SignalDefinition CastCancelSignal;
        [SerializeField] public SignalDefinition CastInterruptSignal;
    }
}
