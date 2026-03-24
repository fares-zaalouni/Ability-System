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
            if (context.TryGetRuntimeSignal(_signal, out var runtimeSignal))
            {
                runtimeSignal.Raise(context);
            }
            runner.Next();
        }
    }
}