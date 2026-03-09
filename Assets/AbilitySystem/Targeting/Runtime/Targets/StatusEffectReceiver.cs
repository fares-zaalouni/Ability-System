using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem.Targeting.Targets
{
    public class StatusEffectReceiver : MonoBehaviour, IStatusEffectReceiver
    {
        protected List<StatusEffect> _activeEffects = new List<StatusEffect>();
        public virtual void ApplyStatusEffectTo(StatusEffect statusEffect, ICaster source = null, int stacks = 1)
        {
            // Check for existing effect to handle stacking
            var existing = _activeEffects.Find(e => e.Definition == statusEffect.Definition);
            Debug.Log($"Applying {statusEffect.Definition} to {gameObject}. Existing effect: {existing}");
            if (existing != null)
            {
                HandleStacking(existing, statusEffect, stacks);
            }
            else
            {
                _activeEffects.Add(statusEffect);
                //definition.OnApply(gameObject); // Initial application (e.g., slow movement)
            }
        }
        
        protected virtual void HandleStacking(StatusEffect existing, StatusEffect newEff, int newStacks)
        {
            /*if (newEff.stackType == StackType.Refresh)
            {
                existing.RemainingDuration = newEff.duration;
            }
            else if (newEff.stackType == StackType.Stack)
            {
                existing.Stacks = Mathf.Min(existing.Stacks + newStacks, newEff.maxStacks);
                existing.RemainingDuration = newEff.duration;
            }*/
        }

        protected virtual void ProcessEffects(float deltaTime)
        {
            // Iterate backwards to safely remove items
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                if (effect.IsExpired)
                {
                    //effect.Definition.OnRemove(gameObject); // Cleanup (e.g., restore speed)
                    _activeEffects.RemoveAt(i);
                    continue;
                }
                effect.Tick(deltaTime, this as IAbilityTarget);    
            }
        }
        private void FixedUpdate()
        {
            ProcessEffects(Time.fixedDeltaTime);
        }
    }
}