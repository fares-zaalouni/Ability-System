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
        // Stored so we can detach before calling Cancel/Interrupt on a sub-runner,
        // preventing the sub-runner's aftermath from re-entering this action.
        private readonly Dictionary<AbilityRunner, (Action onCancelled, Action onInterrupted, Action onCompleted)> _subRunnerCallbacks;
        private AbilityRunner _mainRunner;

        public DoOnSignalAction(
            SignalDefinition triggerSignal,
            SignalDefinition exitSignal,
            List<IAbilityAction> subActions,
            SustainedActionEndAftermath subRunnerExitAftermath,
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
            _activeSubRunners = new List<AbilityRunner>();
            _subRunnerCallbacks = new Dictionary<AbilityRunner, (Action, Action, Action)>();
        }

        public override void Execute(AbilityContext context, AbilityRunner runner)
        {
            _mainRunner = runner;

            if (context.TryGet<RuntimeSignal>(_triggerSignal.ContextKey, out _triggerRuntimeSignal))
                _triggerRuntimeSignal.Subscribe(OnTriggerSignalRaised);
            else
                SignalBus.Subscribe(_triggerSignal, OnTriggerSignalRaised);

            if (context.TryGet<RuntimeSignal>(_exitSignal.ContextKey, out _exitRuntimeSignal))
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
            Action onCancelled   = () => { DetachSubRunnerCallbacks(subRunner); _activeSubRunners.Remove(subRunner); _mainRunner.Cancel(); };
            Action onInterrupted = () => { DetachSubRunnerCallbacks(subRunner); _activeSubRunners.Remove(subRunner); _mainRunner.Interrupt(); };
            Action onCompleted   = () => { DetachSubRunnerCallbacks(subRunner); _activeSubRunners.Remove(subRunner); };

            subRunner.OnCancelled   += onCancelled;
            subRunner.OnInterrupted += onInterrupted;
            subRunner.OnCompleted   += onCompleted;
            _subRunnerCallbacks[subRunner] = (onCancelled, onInterrupted, onCompleted);
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

        // Snapshot + clear the live list so callbacks that fire during cleanup
        // (e.g. OnCompleted removing from the list) don't interfere.
        // Detach callbacks so orphaned sub-runners (None aftermath) can no longer
        // reach _mainRunner after this action has finished.
        // StopWithCancel/StopWithInterrupt never fire events, so there is no
        // re-entry risk regardless of callback wiring.
        private void CleanupSubRunners(SustainedActionEndAftermath aftermath)
        {
            foreach (var subRunner in _activeSubRunners)
            {
                // Always detach: for Cancel/Interrupt this is just dictionary cleanup;
                // for None it's essential — orphaned sub-runners that internally cancel
                // themselves must not reach _mainRunner after this action has finished.
                DetachSubRunnerCallbacks(subRunner);

                if (aftermath == SustainedActionEndAftermath.Cancel)
                    subRunner.StopWithCancel();
                else if (aftermath == SustainedActionEndAftermath.Interrupt)
                    subRunner.StopWithInterrupt();
                // None: sub-runner keeps running, fully disconnected from this pipeline.
            }
            _activeSubRunners.Clear();
        }

        private void DetachSubRunnerCallbacks(AbilityRunner subRunner)
        {
            if (_subRunnerCallbacks.TryGetValue(subRunner, out var callbacks))
            {
                subRunner.OnCancelled   -= callbacks.onCancelled;
                subRunner.OnInterrupted -= callbacks.onInterrupted;
                subRunner.OnCompleted   -= callbacks.onCompleted;
                _subRunnerCallbacks.Remove(subRunner);
            }
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