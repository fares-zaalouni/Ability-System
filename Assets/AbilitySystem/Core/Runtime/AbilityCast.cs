using System;
using System.Collections.Generic;
using AbilitySystem.Utility;
using UnityEngine;

namespace AbilitySystem.Core
{
    public class AbilityCast
    {
        private AbilityRunner _runner;
        public event Action<AbilityContext> OnCompleted;
        public event Action<AbilityContext> OnInterrupted;
        public event Action<AbilityContext> OnCancelled;


        public AbilityCast(ICaster caster, AbilityDefinition definition, IEnumerable<object> initialDependencies = null)
        {
            AbilityContext context = new AbilityContext(caster, initialDependencies);
            if (definition == null)
            {
                AbilityDebug.LogError($"AbilityCast: AbilityDefinition is null for caster {caster}.");
                return;
            }
            List<IAbilityAction> actions = definition.ActionDefinitions.ConvertAll(a => a.CreateRuntimeAction());
            _runner = new AbilityRunner(actions, context);
            _runner.OnCompleted += () => OnCompleted?.Invoke(context);
            _runner.OnInterrupted += () => OnInterrupted?.Invoke(context);
            _runner.OnCancelled += () => OnCancelled?.Invoke(context);
        }

        public void Execute()
        {
            if(_runner == null)
            {
                AbilityDebug.LogError("AbilityCast: Cannot execute because runner is null.");
                return;
            }
            _runner.Next();
        }

        public void Cancel()
        {
            if(_runner == null)
            {
                AbilityDebug.LogError("AbilityCast: Cannot cancel because runner is null.");
                return;
            }
            _runner.Cancel();
        }
        public void Interrupt()
        {
            if(_runner == null)
            {
                AbilityDebug.LogError("AbilityCast: Cannot interrupt because runner is null.");
                return;
            }
            _runner.Interrupt();
        }
    }
}