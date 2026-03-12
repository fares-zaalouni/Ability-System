using System.Collections;
using AbilitySystem.Utility;
using UnityEngine;

namespace AbilitySystem.Core
{
    public class WaitAction : SustainedAction
    {
        private float _duration;
        private float _startTime;
        private Coroutine _waitCoroutine;

        public WaitAction(
        bool isCancellable, 
        bool isInterruptible, 
        SustainedActionEndAftermath cancelAfterMath, 
        SustainedActionEndAftermath interruptAfterMath, 
        float duration
        ) : base(isCancellable, isInterruptible, cancelAfterMath, interruptAfterMath)
        {
            _duration = duration;
        }

        public override void Execute(AbilityContext context, AbilityRunner runner)
        {
            _startTime = Time.time;
            _waitCoroutine = CoroutineRunner.Instance.StartCoroutine(WaitThenNext(_duration, context, runner));
        }

        public override bool Interrupt(AbilityContext context)
        {
            if (!_isInterruptible) return false;
            return Stop(context);
        }

        public override bool Cancel(AbilityContext context)
        {
            if (!_isCancellable) return false;
            return Stop(context);
        }

        private bool Stop(AbilityContext context)
        {
            if (_waitCoroutine != null)
            {
                CoroutineRunner.Instance.StopCoroutine(_waitCoroutine);
                context.Set(ContextKeys.WaitElapsed, false);
                context.Set(ContextKeys.WaitDuration, Time.time - _startTime);
                context.Set(ContextKeys.MaxWaitDuration, _duration);
                return true;
            }
            return false;
        }
        private IEnumerator WaitThenNext(float duration, AbilityContext context, AbilityRunner runner)
        {
            yield return new WaitForSeconds(duration);
            context.Set(ContextKeys.WaitElapsed, true);
            context.Set(ContextKeys.WaitDuration, duration);
            context.Set(ContextKeys.MaxWaitDuration, duration);
            runner.Next();
        }
    }
}