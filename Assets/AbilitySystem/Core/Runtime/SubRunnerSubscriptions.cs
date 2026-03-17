using System;
using System.Collections.Generic;

namespace AbilitySystem.Core
{
    /// <summary>
    /// Tracks sub-runner callback subscriptions so callers can detach safely
    /// before cleanup/stop, avoiding event re-entry and orphaned callback chains.
    /// </summary>
    public sealed class SubRunnerSubscriptions
    {
        private readonly Dictionary<AbilityRunner, (Action onCompleted, Action onCancelled, Action onInterrupted)> _callbacks
            = new Dictionary<AbilityRunner, (Action, Action, Action)>();

        public void Subscribe(
            AbilityRunner subRunner,
            Action onCompleted,
            Action onCancelled,
            Action onInterrupted)
        {
            subRunner.OnCompleted += onCompleted;
            subRunner.OnCancelled += onCancelled;
            subRunner.OnInterrupted += onInterrupted;
            _callbacks[subRunner] = (onCompleted, onCancelled, onInterrupted);
        }

        public void Unsubscribe(AbilityRunner subRunner)
        {
            if (!_callbacks.TryGetValue(subRunner, out var callbacks))
                return;

            subRunner.OnCompleted -= callbacks.onCompleted;
            subRunner.OnCancelled -= callbacks.onCancelled;
            subRunner.OnInterrupted -= callbacks.onInterrupted;
            _callbacks.Remove(subRunner);
        }

        public void UnsubscribeAndApplyAftermath(
            IEnumerable<AbilityRunner> subRunners,
            SustainedActionEndAftermath requestedAftermath,
            SubRunnerCleanupMode cleanupMode)
        {
            foreach (var subRunner in subRunners)
            {
                Unsubscribe(subRunner);

                if (cleanupMode == SubRunnerCleanupMode.DetachAndLetRun)
                    continue;

                if (requestedAftermath == SustainedActionEndAftermath.Cancel)
                {
                    if (cleanupMode == SubRunnerCleanupMode.RespectChildAftermath)
                        subRunner.Cancel();
                    else
                        subRunner.StopSilentlyAsCancelled();
                }
                else if (requestedAftermath == SustainedActionEndAftermath.Interrupt)
                {
                    if (cleanupMode == SubRunnerCleanupMode.RespectChildAftermath)
                        subRunner.Interrupt();
                    else
                        subRunner.StopSilentlyAsInterrupted();
                }
            }
        }
    }
}