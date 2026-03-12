namespace AbilitySystem.Core
{
    public class WaitForSignalAction : SustainedAction
    {
        private SignalDefinition _signal;
        private AbilityRunner _runner;
        private RuntimeSignal _runtimeSignal; // non-null when subscribed to a per-cast signal

        public WaitForSignalAction(SignalDefinition signal, bool isCancellable, bool isInterruptible, SustainedActionEndAftermath cancelAfterMath, SustainedActionEndAftermath interruptAfterMath) : 
            base(isCancellable, isInterruptible, cancelAfterMath, interruptAfterMath)
        {
            _signal = signal;
        }

        public override void Execute(AbilityContext context, AbilityRunner runner)
        {
            _runner = runner;

            // Prefer a per-cast RuntimeSignal written into context by a producer action
            // (e.g. SpawnProjectileAction). If none exists, fall back to the global SignalBus.
            if (context.TryGet<RuntimeSignal>(_signal.ContextKey, out _runtimeSignal))
            {
                _runtimeSignal.Subscribe(OnRuntimeSignalRaised);
            }
            else
            {
                _runtimeSignal = null;
                SignalBus.Subscribe(_signal, OnGlobalSignalRaised);
            }
        }

        public override bool Cancel(AbilityContext context)
        {
            if (!_isCancellable) return false;
            return Stop();
        }

        public override bool Interrupt(AbilityContext context)
        {
            if (!_isInterruptible) return false;
            return Stop();
        }

        private bool Stop()
        {
            if (_runtimeSignal != null)
                _runtimeSignal.Unsubscribe(OnRuntimeSignalRaised);
            else
                SignalBus.Unsubscribe(_signal, OnGlobalSignalRaised);
            return true;
        }

        private void OnRuntimeSignalRaised(AbilityContext context)
        {
            _runtimeSignal.Unsubscribe(OnRuntimeSignalRaised);
            _runner.Next();
        }

        private void OnGlobalSignalRaised(AbilityContext context)
        {
            SignalBus.Unsubscribe(_signal, OnGlobalSignalRaised);
            _runner.Next();
        }
    }
}