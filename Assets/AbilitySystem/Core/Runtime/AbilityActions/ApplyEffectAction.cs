using UnityEngine;
using AbilitySystem.Effects;
using AbilitySystem.Targeting;

namespace AbilitySystem.Core
{
    public class ApplyEffectAction : IAbilityAction
    {
        private AbilityEffectDefinition _abilityEffectDefinition;

        public ApplyEffectAction(AbilityEffectDefinition abilityEffectDefinition)
        {
            _abilityEffectDefinition = abilityEffectDefinition;
        }

        public void Execute(AbilityContext context, AbilityRunner runner)
        {
            int totalTargets = context.Targets.Count;
            int appliedCount = 0;
            int skippedCount = 0;
            int failedCount = 0;

            foreach (IAbilityTarget target in context.Targets)
            {
                var applyResult = _abilityEffectDefinition.CreateEffect(context.Caster).ApplyTo(target);
                switch (applyResult)
                {
                    case AbilityEffectApplyResult.Applied:
                        appliedCount++;
                        break;
                    case AbilityEffectApplyResult.SkippedUnsupportedTarget:
                        skippedCount++;
                        break;
                    case AbilityEffectApplyResult.Failed:
                        failedCount++;
                        break;
                }
            }

            context.Set(new EffectApplySummary(totalTargets, appliedCount, skippedCount, failedCount));

            if (Debug.isDebugBuild && totalTargets > 0 && appliedCount == 0)
            {
                Debug.LogWarning(
                    $"ApplyEffectAction: Effect '{_abilityEffectDefinition.name}' applied to 0/{totalTargets} targets. " +
                    $"Skipped={skippedCount}, Failed={failedCount}.");
            }

            runner.Next();
        }
    }
}