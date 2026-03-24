using UnityEngine;

namespace AbilitySystem.Targeting
{
    [CreateAssetMenu(fileName = "SingleTargetStrategyDefinition", menuName = "Ability System/Targeting Strategies/Single Target")]
    public class SingleTargetStrategyDefinition : TargetingStrategyDefinition
    {
        [SerializeField] private float _precisionRadius = 0.1f;
        [SerializeField] private bool _isProjectileHit;
        public override ITargetingStrategy CreateRuntimeStrategy()
        {
            return new SingleTargetStrategy(_targetLayerMask, _precisionRadius, _isProjectileHit);
        }
    }
}