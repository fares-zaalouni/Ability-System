using AbilitySystem.Resources;
using UnityEngine;

namespace AbilitySystem.Costs
{
    [CreateAssetMenu(fileName = "AbilityCostDefinition", menuName = "Ability System/Costs/Ability Cost Definition")]
    public class AbilityCostDefinition : ScriptableObject
    {
        [SerializeField] private float _amount;
        [SerializeField] private ResourceDefinition _resourceCost;
        public AbilityCost CreateRuntimeCost()
        {
            return new AbilityCost(_resourceCost.ResourceName, _amount);
        }
    }
}