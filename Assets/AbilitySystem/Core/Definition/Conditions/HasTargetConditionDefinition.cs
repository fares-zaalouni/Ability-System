using UnityEngine;

namespace AbilitySystem.Core
{
    [CreateAssetMenu(fileName = "HasTargetConditionDefinition", menuName = "Ability System/Conditions/Has Target")]
    public class HasTargetConditionDefinition : ConditionDefinition
    {
        [SerializeField] private int _minTargets = 1;

        public override bool Evaluate(AbilityContext context)
        {
            return context.Targets.Count >= _minTargets;
        }
    }
}