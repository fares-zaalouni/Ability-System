namespace AbilitySystem.Core
{
    public class RaiseSignalAction : IAbilityAction
    {
        private SignalDefinition _signal;

        public RaiseSignalAction(SignalDefinition signal)
        {
            _signal = signal;
        }
        public void Execute(AbilityContext context, AbilityRunner runner)
        {
            SignalBus.Raise(_signal, context);
            runner.Next();
        }
    }
}