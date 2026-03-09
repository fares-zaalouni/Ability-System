using System.Collections.Generic;
using UnityEngine;
using AbilitySystem.Resources;
using AbilitySystem.Costs;

namespace AbilitySystem.Core
{
    [CreateAssetMenu(fileName = "AbilityDefinition", menuName = "Ability System/AbilityDefinition")]
    public class AbilityDefinition : ScriptableObject
    {
        [SerializeField] public string abilityName;
        [SerializeField] public float cooldown;
        [SerializeField] public List<AbilityCostDefinition> costs;
        [SerializeField] public List<AbilityActionDefinition> actionDefinitions;
    }
}
