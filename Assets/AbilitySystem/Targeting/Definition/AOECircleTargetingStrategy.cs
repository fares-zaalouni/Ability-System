using UnityEngine;

namespace AbilitySystem.Targeting
{
    [CreateAssetMenu(fileName = "AOECircleTargetingStrategy", menuName = "Ability System/Targeting Strategies/AOECircleTargetingStrategy")]
    public class AOECircleTargetingStrategy : TargetingStrategyDefinition
    {
        [SerializeField] private float _radius;

        public override ITargetingStrategy CreateRuntimeStrategy()
        {
            return new AOECircleStrategy(_radius, _targetLayerMask);
        }
    }
}
