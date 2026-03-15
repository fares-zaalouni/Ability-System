using System.Collections.Generic;
using UnityEngine;
using AbilitySystem.Targeting;

namespace AbilitySystem.Core
{
    public class AbilityContext
    {
        public ICaster Caster { get; private set; }
        public List<IAbilityTarget> Targets { get; private set; }
        private readonly Dictionary<string, object> _blackboard;
        public AbilityContext(ICaster caster, Dictionary<string, object> initialBlackboard = null)
        {
            Caster = caster;
            Targets = new List<IAbilityTarget>();
            _blackboard = initialBlackboard ?? new Dictionary<string, object>();

        }
        public void Set<T>(string key, T value) => _blackboard[key] = value;

        public bool TryGet<T>(string key, out T value)
        {
            if (_blackboard.TryGetValue(key, out var raw) && raw is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
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
            var forkedBlackboard = new Dictionary<string, object>();
            foreach (var kvp in _blackboard)
                forkedBlackboard[kvp.Key] = kvp.Value;

            var fork = new AbilityContext(Caster, forkedBlackboard);
            fork.SetTargets(new List<IAbilityTarget>(Targets));
            return fork;
        }
    }
}