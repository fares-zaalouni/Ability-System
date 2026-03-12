using UnityEngine;

namespace AbilitySystem.Core
{
    public enum SustainedActionEndAftermath
    {
        None,
        Cancel,
        Interrupt
    }
    public abstract class SustainedActionDefinition : AbilityActionDefinition
    {
        [SerializeField] protected bool IsCancellable = false;
        [SerializeField] protected bool IsInterruptible = false;
        [SerializeField] protected SustainedActionEndAftermath CancelAfterMath = SustainedActionEndAftermath.None;
        [SerializeField] protected SustainedActionEndAftermath InterruptAfterMath = SustainedActionEndAftermath.None;
    }
}