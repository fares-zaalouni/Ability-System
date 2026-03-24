using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem.Core
{
    public class DoOnSignalAction : SustainedAction
    {
        private readonly SignalDefinition _triggerSignal;
        private readonly SignalDefinition _exitSignal;
        private readonly SustainedActionEndAftermath _subRunnerExitAftermath;
        private readonly List<IAbilityAction> _subActions;
        private RuntimeSignal _triggerRuntimeSignal;
        private RuntimeSignal _exitRuntimeSignal;
        private readonly List<AbilityRunner> _activeSubRunners;
        private readonly SubRunnerSubscriptions _subRunnerSubscriptions;
        private readonly SubRunnerCleanupMode _subRunnerCleanupMode;
        private AbilityRunner _mainRunner;

        public DoOnSignalAction(
            SignalDefinition triggerSignal,
            SignalDefinition exitSignal,
            List<IAbilityAction> subActions,
            SustainedActionEndAftermath subRunnerExitAftermath,
            SubRunnerCleanupMode subRunnerCleanupMode,
            bool isCancellable,
            bool isInterruptible,
            SustainedActionEndAftermath cancelAfterMath,
            SustainedActionEndAftermath interruptAfterMath) :
            base(isCancellable, isInterruptible, cancelAfterMath, interruptAfterMath)
        {
            _triggerSignal = triggerSignal;
            _exitSignal = exitSignal;
            _subActions = subActions;
            _subRunnerExitAftermath = subRunnerExitAftermath;
            _subRunnerCleanupMode = subRunnerCleanupMode;
            _activeSubRunners = new List<AbilityRunner>();
            _subRunnerSubscriptions = new SubRunnerSubscriptions();
        }

        public override void Execute(AbilityContext context, AbilityRunner runner)
        {
            _mainRunner = runner;

            if (context.TryGetRuntimeSignal(_triggerSignal, out _triggerRuntimeSignal))
                _triggerRuntimeSignal.Subscribe(OnTriggerSignalRaised);
            else
                SignalBus.Subscribe(_triggerSignal, OnTriggerSignalRaised);

            if (context.TryGetRuntimeSignal(_exitSignal, out _exitRuntimeSignal))
                _exitRuntimeSignal.Subscribe(OnExitSignalRaised);
            else
                SignalBus.Subscribe(_exitSignal, OnExitSignalRaised);
        }

        private void OnTriggerSignalRaised(AbilityContext context)
        {
            // Fork the context so this sub-runner gets an independent snapshot.
            // Without this, a second trigger hit would overwrite Targets and blackboard
            // values in the shared context, corrupting all still-running sub-runners.
            AbilityRunner subRunner = new AbilityRunner(_subActions, context.Fork());

            // Store lambdas so we can detach them by reference before triggering cleanup.
            Action onCancelled = () =>
            {
                _subRunnerSubscriptions.Unsubscribe(subRunner);
                _activeSubRunners.Remove(subRunner);
                _mainRunner.Cancel();
            };
            Action onInterrupted = () =>
            {
                _subRunnerSubscriptions.Unsubscribe(subRunner);
                _activeSubRunners.Remove(subRunner);
                _mainRunner.Interrupt();
            };
            Action onCompleted = () =>
            {
                _subRunnerSubscriptions.Unsubscribe(subRunner);
                _activeSubRunners.Remove(subRunner);
            };
            _subRunnerSubscriptions.Subscribe(subRunner, onCompleted, onCancelled, onInterrupted);
            _activeSubRunners.Add(subRunner);

            subRunner.Next();
        }

        private void OnExitSignalRaised(AbilityContext context)
        {
            UnsubscribeFromSignals();
            CleanupSubRunners(_subRunnerExitAftermath);
            _mainRunner.Next();
        }

        public override bool Cancel(AbilityContext context)
        {
            if (!_isCancellable) return false;
            UnsubscribeFromSignals();
            CleanupSubRunners(_cancelAfterMath);
            return true;
        }

        public override bool Interrupt(AbilityContext context)
        {
            if (!_isInterruptible) return false;
            UnsubscribeFromSignals();
            CleanupSubRunners(_interruptAfterMath);
            return true;
        }

        // Detach callbacks first, then apply configured child cleanup mode.
        private void CleanupSubRunners(SustainedActionEndAftermath aftermath)
        {
            _subRunnerSubscriptions.UnsubscribeAndApplyAftermath(
                _activeSubRunners,
                aftermath,
                _subRunnerCleanupMode);
            _activeSubRunners.Clear();
        }

        private void UnsubscribeFromSignals()
        {
            if (_triggerRuntimeSignal != null)
                _triggerRuntimeSignal.Unsubscribe(OnTriggerSignalRaised);
            else
                SignalBus.Unsubscribe(_triggerSignal, OnTriggerSignalRaised);

            if (_exitRuntimeSignal != null)
                _exitRuntimeSignal.Unsubscribe(OnExitSignalRaised);
            else
                SignalBus.Unsubscribe(_exitSignal, OnExitSignalRaised);
        }
    }
}