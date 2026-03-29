using System.Collections.Generic;
using AbilitySystem.Core;
using AbilitySystem.Targeting;

namespace AbilitySystem.Effects
{
    public class OverTimeEffectGroup
    {
        public readonly List<OverTimeEffect> Effects;
        public int EffectCount => Effects.Count;

        public OverTimeEffectGroup()
        {
            Effects = new List<OverTimeEffect>();
        }

        public void AddEffect(OverTimeEffect effect)
        {
            Effects.Add(effect);
        }
        public void RemoveEffect(OverTimeEffect effect)
        {
            Effects.Remove(effect);
        }
        public void ReplaceEffect(OverTimeEffect oldEffect, OverTimeEffect newEffect)
        {
            int index = Effects.IndexOf(oldEffect);
            if (index != -1)
                Effects[index] = newEffect;
        }
       
        public OverTimeEffect GetOldestEffect()
        {
            if (Effects == null || Effects.Count == 0) return null;
            return Effects[0];
        }
        public bool TryGetOldestEffectFromSource(ICaster source, out OverTimeEffect effect)
        {
            effect = null;
            if (source == null) return false;
            var effectsSnapshot = new List<OverTimeEffect>(Effects);
            foreach (var e in effectsSnapshot)
            {
                if (e.Context.Caster == source)
                {
                    effect = e;
                    return true;
                }
            }
            return false;
        }

        public OverTimeEffect GetNewestEffect()
        {
            if (Effects == null || Effects.Count == 0) return null;
            return Effects[Effects.Count - 1];
        }
        public bool TryGetNewestEffectFromSource(ICaster source, out OverTimeEffect effect)
        {
            effect = null;
            if (source == null) return false;
            var effectsSnapshot = new List<OverTimeEffect>(Effects);
            for (int i = effectsSnapshot.Count - 1; i >= 0; i--)
            {
                if (effectsSnapshot[i].Context.Caster == source)
                {
                    effect = effectsSnapshot[i];
                    return true;
                }
            }
            return false;
        }
        public OverTimeEffect GetEffectAt(int index)
        {
            if (Effects == null || index < 0 || index >= Effects.Count) return null;
            return Effects[index];
        }
        public OverTimeEffect GetEffectWithLeastTimeRemaining()
        {
            if (Effects == null || Effects.Count == 0) return null;
            OverTimeEffect least = Effects[0];
            foreach (var effect in Effects)
            {
                if (effect.RemainingDuration < least.RemainingDuration)
                    least = effect;
            }
            return least;
        }
        public bool TryGetEffectWithLeastTimeRemainingFromSource(ICaster source, out OverTimeEffect effect)
        {
            effect = null;
            if (source == null) return false;
            var effectsSnapshot = new List<OverTimeEffect>(Effects);
            OverTimeEffect least = null;
            foreach (var e in effectsSnapshot)
            {
                if (e.Context.Caster == source)
                {
                    if (least == null || e.RemainingDuration < least.RemainingDuration)
                        least = e;
                }
            }
            if (least != null)
            {
                effect = least;
                return true;
            }
            return false;
        }
        public OverTimeEffect GetEffectWithMostTimeRemaining()
        {
            if (Effects == null || Effects.Count == 0) return null;
            OverTimeEffect most = Effects[0];
            foreach (var effect in Effects)
            {
                if (effect.RemainingDuration > most.RemainingDuration)
                    most = effect;
            }
            return most;
        }
        public bool TryGetEffectWithMostTimeRemainingFromSource(ICaster source, out OverTimeEffect effect)
        {
            effect = null;
            if (source == null) return false;
            var effectsSnapshot = new List<OverTimeEffect>(Effects);
            OverTimeEffect most = null;
            foreach (var e in effectsSnapshot)
            {
                if (e.Context.Caster == source)
                {
                    if (most == null || e.RemainingDuration > most.RemainingDuration)
                        most = e;
                }
            }
            if (most != null)
            {
                effect = most;
                return true;
            }
            return false;
        }
        public OverTimeEffect GetEffectWithLeastStacks()
        {
            if (Effects == null || Effects.Count == 0) return null;
            OverTimeEffect least = Effects[0];
            foreach (var effect in Effects)
            {
                if (effect.Stacks < least.Stacks)
                    least = effect;
            }
            return least;
        }
        public bool TryGetEffectWithLeastStacksFromSource(ICaster source, out OverTimeEffect effect)
        {
            effect = null;
            if (source == null) return false;
            var effectsSnapshot = new List<OverTimeEffect>(Effects);
            OverTimeEffect least = null;
            foreach (var e in effectsSnapshot)
            {
                if (e.Context.Caster == source)
                {
                    if (least == null || e.Stacks < least.Stacks)
                        least = e;
                }
            }
            if (least != null)
            {
                effect = least;
                return true;
            }
            return false;
        }
        public OverTimeEffect GetEffectWithMostStacks()
        {
            if (Effects == null || Effects.Count == 0) return null;
            OverTimeEffect most = Effects[0];
            foreach (var effect in Effects)
            {
                if (effect.Stacks > most.Stacks)
                    most = effect;
            }
            return most;
        }
        public bool TryGetEffectWithMostStacksFromSource(ICaster source, out OverTimeEffect effect)
        {
            effect = null;
            if (source == null) return false;
            var effectsSnapshot = new List<OverTimeEffect>(Effects);
            OverTimeEffect most = null;
            foreach (var e in effectsSnapshot)
            {
                if (e.Context.Caster == source)
                {
                    if (most == null || e.Stacks > most.Stacks)
                        most = e;
                }
            }
            if (most != null)
            {
                effect = most;
                return true;
            }
            return false;
        }
        
        public void RefreshAllDurations()
        {
            foreach (var effect in Effects)
            {
                effect.RefreshDuration();
            }
        }

        // Variations that operate only on effects from the same source
        public void RefreshDurationsBySource(Core.ICaster source)
        {
            if (source == null) return;
            foreach (var e in Effects)
            {
                if (e.Context.Caster == source)
                    e.RefreshDuration();
            }
        }

        public void AddStacks(OverTimeEffect effect, int amount)
        {
            if(Effects.Contains(effect))
                effect.AddStacks(amount);
        }
        public void AddStacksToAll(int amount)
        {
            foreach (var effect in Effects)
            {
                effect.AddStacks(amount);
            }
        }

        // Add stacks only to effects from the same source
        public bool TryAddStacksBySource(ICaster source, int amount)
        {
            if (source == null) return false;
            bool added = false;
            foreach (var e in Effects)
            {
                if (e.Context.Caster == source)
                {
                    e.AddStacks(amount);
                    added = true;
                }
            }
            return added;
        }
        public void RemoveStacks(OverTimeEffect effect, int amount)
        {
            if(Effects.Contains(effect))
                effect.RemoveStacks(amount);
        }
        public void RemoveStacksFromAll(int amount)
        {
            foreach (var effect in Effects)
            {
                effect.RemoveStacks(amount);
            }
        }

        // Remove stacks only from effects from the same source
        public void RemoveStacksBySource(ICaster source, int amount)
        {
            if (source == null) return;
            foreach (var e in Effects)
            {
                if (e.Context.Caster == source)
                    e.RemoveStacks(amount);
            }
        }
        public void ExtendDuration(OverTimeEffect effect, float amount)
        {
            if(Effects.Contains(effect))
                effect.ExtendDuration(amount);
        }
        public void ExtendAllDurations(float amount)
        {
            foreach (var effect in Effects)
            {
                effect.ExtendDuration(amount);
            }
        }

        // Extend durations only for effects from the same source

        public void ExtendDurationsBySource(Core.ICaster source, float amount)
        {
            if (source == null) return;
            foreach (var e in Effects)
            {
                if (e.Context.Caster == source)
                    e.ExtendDuration(amount);
            }
        }

        
        public void TickAll(float deltaTime, IAbilityTarget target)
        {
            var effectsCopy = new List<OverTimeEffect>(Effects);
            foreach (var effect in effectsCopy)
            {
                effect.Tick(deltaTime, target); 
            }
        }
        
        public void ReApplyModifiers(IAbilityTarget target)
        {
            foreach (var effect in Effects)
            {
                effect.ApplyModifiers(target);
            }
        }
    }
}