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
                float elapsedDuration = Time.time - _startTime;
                context.Set(new WaitStatus(false, elapsedDuration, _duration));
                return true;
            }
            return false;
        }
        private IEnumerator WaitThenNext(float duration, AbilityContext context, AbilityRunner runner)
        {
            yield return new WaitForSeconds(duration);
            context.Set(new WaitStatus(true, duration, duration));
            runner.Next();
        }
    }
}