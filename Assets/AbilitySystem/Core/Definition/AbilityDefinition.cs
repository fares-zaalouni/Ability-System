using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityDefinition", menuName = "Ability System/AbilityDefinition")]
public class AbilityDefinition : ScriptableObject
{
    [SerializeField] public string abilityName;
    [SerializeField] public float cooldown;
    [SerializeField] public List<Cost> costs;
    [SerializeField] public List<AbilityActionDefinition> actionDefinitions;
}
