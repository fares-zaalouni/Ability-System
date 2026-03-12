namespace AbilitySystem.Core
{
    public abstract class SustainedAction : IAbilityAction
    {
        protected bool _isCancellable;
        protected bool _isInterruptible;
        protected SustainedActionEndAftermath _cancelAfterMath;
        protected SustainedActionEndAftermath _interruptAfterMath;

        public SustainedActionEndAftermath CancelAftermath => _cancelAfterMath;
        public SustainedActionEndAftermath InterruptAftermath => _interruptAfterMath;

        protected SustainedAction(bool isCancellable, bool isInterruptible, SustainedActionEndAftermath cancelAfterMath, SustainedActionEndAftermath interruptAfterMath)
        {
            _isCancellable = isCancellable;
            _isInterruptible = isInterruptible;
            _cancelAfterMath = cancelAfterMath;
            _interruptAfterMath = interruptAfterMath;
        }
        public abstract void Execute(AbilityContext context, AbilityRunner runner);
        public abstract bool Cancel(AbilityContext context);
        public abstract bool Interrupt(AbilityContext context);
    }
}