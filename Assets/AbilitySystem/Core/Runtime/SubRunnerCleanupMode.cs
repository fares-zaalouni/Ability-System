namespace AbilitySystem.Core
{
    public enum SubRunnerCleanupMode
    {
        // Detach callbacks, then ask the child runner to process Cancel/Interrupt
        // through its normal action aftermath flow.
        RespectChildAftermath = 0,

        // Detach callbacks, then force-stop the child runner without firing events
        // or executing child aftermath propagation.
        ForceSilentStop = 1,

        // Detach callbacks and leave child runners alive and disconnected.
        DetachAndLetRun = 2
    }
}