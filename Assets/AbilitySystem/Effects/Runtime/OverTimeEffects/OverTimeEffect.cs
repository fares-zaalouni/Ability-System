using AbilitySystem.Targeting;
using AbilitySystem.Core;
using System;
using System.Collections.Generic;

namespace AbilitySystem.Effects
{
    public abstract class OverTimeEffect : IAbilityEffect
    {
        public OverTimeEffectDefinition _definition { get; private set; }
        public AbilityContext Context { get; private set; }
        public bool ApplyOnce { get; private set; }
        public float TickInterval { get; private set; }
        public int TickCount { get; private set; }
        public float TimeSinceLastTick { get; private set; }
        public float RemainingDuration { get; private set; }
        public float TotalDuration { get; private set; }
        public int Stacks { get; private set; }
        public int MaxStacks { get; private set; }
        public IStackingPolicy StackingPolicy { get; private set; }
        public int EffectTypeId => _definition.Id;
        public Guid Id { get; } = Guid.NewGuid();

        private bool _isExpired = false;

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

        public OverTimeEffect(OverTimeEffectDefinition definition, float duration, float tickInterval, int stacks, int maxStacks, AbilityContext context, bool applyOnce = false)
        {
            _definition = definition;
            RemainingDuration = duration;
            TotalDuration = duration;
            TimeSinceLastTick = tickInterval; // So it applies immediately on first tick
            TickInterval = tickInterval;
            ApplyOnce = applyOnce;
            Stacks = stacks;
            MaxStacks = maxStacks;
            StackingPolicy = definition.StackingPolicy.CreateRuntimeStackingStrategy();
            Context = context;
        }
        
        public void Tick(float deltaTime, IAbilityTarget target)
        {
            RemainingDuration -= deltaTime;
            TimeSinceLastTick += deltaTime;
            TickCount++;
            EffectTick?.Invoke();
            
            if (ApplyOnce)
            {
                ApplyTickTo(target);
                ApplyOnce = false; // Ensure it only applies once
            }
            else
            {
                if (TimeSinceLastTick >= TickInterval && RemainingDuration > 0f)
                {
                    ApplyTickTo(target);
                    TimeSinceLastTick -= TickInterval;
                }
                if(RemainingDuration <= 0f && !_isExpired)
                {
                    _isExpired = true;
                    EffectExpired?.Invoke();
                }
            }
        }

        public abstract void ApplyTickTo(IAbilityTarget target);

        public AbilityEffectApplyResult ApplyTo(IAbilityTarget target)
        {
            if (target == null)
                return AbilityEffectApplyResult.Failed;

            OnEffectApplied();
            RegisterToTarget(target);
            ApplyModifiers(target);

            return AbilityEffectApplyResult.Applied;
        }

        public abstract void ApplyModifiers(IAbilityTarget target);
        public void RegisterToTarget(IAbilityTarget target)
        {
            OverTimeEffectLifetimeManager.Instance.RegisterOverTimeEffect(target, this);
        }
        public void UnregisterFromTarget(IAbilityTarget target)
        {
            OverTimeEffectLifetimeManager.Instance.UnregisterOverTimeEffect(target, this);
        }

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

        public virtual void ExtendDuration(float amount)
        {
            RemainingDuration += amount;
            TotalDuration += amount;
            EffectRefreshed?.Invoke();
        }
    }
}