using AbilitySystem.Resources;
using Unity.Mathematics;
using UnityEngine;

namespace AbilitySystem.Costs
{
    [CreateAssetMenu(fileName = "AbilityCostDefinition", menuName = "Ability System/Costs/Ability Cost Definition")]
    public class AbilityCostDefinition : ScriptableObject
    {
        [SerializeField] private float _amount;
        [SerializeField] private ResourceDefinition _resourceCost;
        public Cost CreateRuntimeCost()
        {
            return new Cost(_resourceCost.ResourceName, _amount);
        }
    }
}