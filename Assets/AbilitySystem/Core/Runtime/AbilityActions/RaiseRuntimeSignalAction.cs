namespace AbilitySystem.Core
{
    public class RaiseRuntimeSignalAction : IAbilityAction
    {
        private SignalDefinition _signal;

        public RaiseRuntimeSignalAction(SignalDefinition signal)
        {
            _signal = signal;
        }
        public void Execute(AbilityContext context, AbilityRunner runner)
        {
            if (context.TryGet<RuntimeSignal>(_signal.Id, out var runtimeSignal))
            {
                runtimeSignal.Raise(context);
            }
            runner.Next();
        }
    }
}