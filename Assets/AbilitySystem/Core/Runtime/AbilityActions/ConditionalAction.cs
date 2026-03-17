using System.Collections.Generic;
using System;

namespace AbilitySystem.Core
{
    public class ConditionalAction : IAbilityAction
    {
        private readonly ConditionDefinition _condition;
        private readonly List<IAbilityAction> _trueActions;
        private readonly List<IAbilityAction> _falseActions;
        private AbilityRunner _subRunner;
        private readonly SubRunnerSubscriptions _subRunnerSubscriptions;

        public ConditionalAction(ConditionDefinition condition, List<IAbilityAction> trueActions, List<IAbilityAction> falseActions)
        {
            _condition = condition;
            _trueActions = trueActions;
            _falseActions = falseActions;
            _subRunnerSubscriptions = new SubRunnerSubscriptions();
        }

        public void Execute(AbilityContext context, AbilityRunner runner)
        {
            var branch = _condition.Evaluate(context) ? _trueActions : _falseActions;
            _subRunner = new AbilityRunner(branch, context);

            Action onSubCompleted = () =>
            {
                DetachSubRunnerCallbacks();
                runner.Next();
            };
            Action onSubCancelled = () =>
            {
                DetachSubRunnerCallbacks();
                runner.Cancel();
            };
            Action onSubInterrupted = () =>
            {
                DetachSubRunnerCallbacks();
                runner.Interrupt();
            };
            _subRunnerSubscriptions.Subscribe(_subRunner, onSubCompleted, onSubCancelled, onSubInterrupted);
            _subRunner.Next();
        }

        private void DetachSubRunnerCallbacks()
        {
            if (_subRunner == null) return;
            _subRunnerSubscriptions.Unsubscribe(_subRunner);
            _subRunner = null;
        }
    }
}