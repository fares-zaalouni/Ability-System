using UnityEngine;

namespace AbilitySystem.Attributes
{
    [CreateAssetMenu(fileName = "AbilityCostDefinition", menuName = "Ability System/Costs/Ability Cost Definition")]
    public class AbilityCostDefinition : ScriptableObject
    {
        [SerializeField] private float _amount;
        [SerializeField] private AttributeDefinition _resourceCost;
        public AbilityCost CreateRuntimeCost()
        {
            return new AbilityCost(_resourceCost.AttributeType, _amount);
        }
    }
}