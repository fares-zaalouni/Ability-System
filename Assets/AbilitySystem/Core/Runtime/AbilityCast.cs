using System;
using System.Collections.Generic;

namespace AbilitySystem.Core
{
    public class AbilityCast
    {
        private AbilityRunner _runner;
        public event Action<AbilityContext> OnCompleted;
        public event Action<AbilityContext> OnInterrupted;
        public event Action<AbilityContext> OnCancelled;


        public AbilityCast(IResourceBearer caster, AbilityDefinition definition, Blackboard initialBlackboard = null)
        {
            AbilityContext context = new AbilityContext(caster, initialBlackboard);
            List<IAbilityAction> actions = definition.ActionDefinitions.ConvertAll(a => a.CreateRuntimeAction());
            _runner = new AbilityRunner(actions, context);
            _runner.OnCompleted += () => OnCompleted?.Invoke(context);
            _runner.OnInterrupted += () => OnInterrupted?.Invoke(context);
            _runner.OnCancelled += () => OnCancelled?.Invoke(context);
        }

        public void Execute()
        {
            _runner.Next();
        }

        public void Cancel()
        {
            _runner.Cancel();
        }
        public void Interrupt()
        {
            _runner.Interrupt();
        }
    }
}