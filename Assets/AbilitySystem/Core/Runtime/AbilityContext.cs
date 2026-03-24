using System.Collections.Generic;
using System;
using AbilitySystem.Targeting;

namespace AbilitySystem.Core
{
    public class AbilityContext
    {
        public ICaster Caster { get; private set; }
        public List<IAbilityTarget> Targets { get; private set; }
        private readonly Dictionary<Type, object> _typedBlackboard;

        public AbilityContext(ICaster caster, IEnumerable<object> initialDependencies = null)
        {
            Caster = caster;
            Targets = new List<IAbilityTarget>();
            _typedBlackboard = new Dictionary<Type, object>();

            if (initialDependencies == null)
                return;

            foreach (var dependency in initialDependencies)
            {
                if (dependency == null)
                    continue;

                _typedBlackboard[dependency.GetType()] = dependency;
            }
        }

        public void Set<T>(T value)
        {
            _typedBlackboard[typeof(T)] = value;
        }

        public void SetRuntimeSignal(SignalDefinition signalDefinition, RuntimeSignal signal)
        {
            if (!_typedBlackboard.TryGetValue(typeof(RuntimeSignalRegistry), out var rawRegistry) || rawRegistry is not RuntimeSignalRegistry registry)
            {
                registry = new RuntimeSignalRegistry();
                _typedBlackboard[typeof(RuntimeSignalRegistry)] = registry;
            }

            registry.Set(signalDefinition, signal);
        }

        public bool TryGet<T>(out T value)
        {
            if (_typedBlackboard.TryGetValue(typeof(T), out var raw) && raw is T typed)
            {
                value = typed;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetRuntimeSignal(SignalDefinition signalDefinition, out RuntimeSignal signal)
        {
            signal = null;
            if (!_typedBlackboard.TryGetValue(typeof(RuntimeSignalRegistry), out var rawRegistry) || rawRegistry is not RuntimeSignalRegistry registry)
                return false;

            return registry.TryGet(signalDefinition, out signal);
        }

        
        public void SetTargets(List<IAbilityTarget> targets)
        {
            Targets = targets;
        }

        // Creates an independent copy of this context at this moment in time.
        // The new context has its own Targets list and its own blackboard, so
        // future mutations by other actions (e.g. a second projectile hit) do
        // not affect a sub-runner that already captured a fork.
        public AbilityContext Fork()
        {
            var forkedTypedBlackboard = new Dictionary<Type, object>();
            foreach (var kvp in _typedBlackboard)
                forkedTypedBlackboard[kvp.Key] = kvp.Value;

            var fork = new AbilityContext(Caster);
            fork.SetTargets(new List<IAbilityTarget>(Targets));
            foreach (var kvp in forkedTypedBlackboard)
                fork._typedBlackboard[kvp.Key] = kvp.Value;
            return fork;
        }
    }

    public sealed class RuntimeSignalRegistry
    {
        private readonly Dictionary<SignalDefinition, RuntimeSignal> _signals = new Dictionary<SignalDefinition, RuntimeSignal>();

        public void Set(SignalDefinition signalDefinition, RuntimeSignal signal)
        {
            _signals[signalDefinition] = signal;
        }

        public bool TryGet(SignalDefinition signalDefinition, out RuntimeSignal signal)
        {
            return _signals.TryGetValue(signalDefinition, out signal);
        }
    }
}