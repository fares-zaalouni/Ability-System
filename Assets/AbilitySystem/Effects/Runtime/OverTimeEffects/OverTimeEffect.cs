using AbilitySystem.Targeting;
using AbilitySystem.Core;
using System;
using System.Collections.Generic;

namespace AbilitySystem.Effects
{
    public abstract class OverTimeEffect : IAbilityEffect
    {
        public AbilityEffectDefinition _definition { get; private set; }
        protected ICaster _source;
        public bool ApplyOnce { get; private set; }
        public float TickInterval { get; private set; }
        public int TickCount { get; private set; }
        public float TimeSinceLastTick { get; private set; }
        public float RemainingDuration { get; private set; }
        public float TotalDuration { get; private set; }
        public int Stacks { get; private set; }
        public int MaxStacks { get; private set; }


        public event Action EffectApplied;
        public event Action EffectExpired;
        public event Action EffectStacked;
        public event Action EffectUnstacked;
        public event Action EffectRefreshed;
        public event Action EffectTick;

        protected virtual void OnEffectApplied() => EffectApplied?.Invoke();
        protected virtual void OnEffectExpired() => EffectExpired?.Invoke();
        protected virtual void OnEffectStacked() => EffectStacked?.Invoke();
        protected virtual void OnEffectUnstacked() => EffectUnstacked?.Invoke();
        protected virtual void OnEffectRefreshed() => EffectRefreshed?.Invoke();
        protected virtual void OnEffectTick() => EffectTick?.Invoke();

        public OverTimeEffect(AbilityEffectDefinition definition, float duration, float tickInterval, bool applyOnce, int stacks, ICaster source)
        {
            _definition = definition;
            RemainingDuration = duration;
            TotalDuration = duration;
            TimeSinceLastTick = tickInterval; // So it applies immediately on first tick
            TickInterval = tickInterval;
            ApplyOnce = applyOnce;
            Stacks = stacks;
            MaxStacks = stacks;
            _source = source;
        }
        
        public int Id => _definition.Id;
        public void Tick(float deltaTime, IAbilityTarget target)
        {
            RemainingDuration -= deltaTime;
            TimeSinceLastTick += deltaTime;
            TickCount++;
            EffectTick?.Invoke();
            
            if (ApplyOnce)
            {
                ApplyTo(target);
                ApplyOnce = false; // Ensure it only applies once
            }
            else
            {
                if (TimeSinceLastTick >= TickInterval && RemainingDuration > 0f)
                {
                    ApplyTo(target);
                    TimeSinceLastTick -= TickInterval;
                }
            }
        }

        public abstract void ApplyTo(IAbilityTarget target);
        
        public abstract void HandleStacking(IAbilityTarget target, List<OverTimeEffect> existingEffects);
        public abstract void HandleExpiration(IAbilityTarget target, List<OverTimeEffect> existingEffects);


        public virtual void AddStacks(int amount)
        {
            Stacks += amount;
            EffectStacked?.Invoke();
        }

        public virtual void RemoveStacks(int amount)
        {
            Stacks = Math.Max(Stacks - amount, 0);
            EffectUnstacked?.Invoke();
        }

        public virtual void RefreshDuration()
        {
            RemainingDuration = TotalDuration;
            EffectRefreshed?.Invoke();
        }
    }
}