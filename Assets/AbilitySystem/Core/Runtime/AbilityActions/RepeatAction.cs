using System.Collections;
using System.Collections.Generic;
using AbilitySystem.Utility;
using UnityEngine;

namespace AbilitySystem.Core
{
    public class RepeatAction : SustainedAction
    {
        private float _tickInterval;
        private float _duration;
        private List<AbilityRunner> _subRunners;
        private List<IAbilityAction> _actions;
        private Coroutine _coroutine;
        private int _tickedTimes;

        public RepeatAction(
            float tickInterval,
            float duration,
            List<IAbilityAction> actions,
            bool isCancellable, 
            bool isInterruptible, 
            SustainedActionEndAftermath cancelAfterMath, 
            SustainedActionEndAftermath interruptAfterMath) : 
        base(isCancellable, isInterruptible, cancelAfterMath, interruptAfterMath)
        {
            _tickInterval = tickInterval;
            _duration = duration;
            _actions = actions;
            _subRunners = new List<AbilityRunner>();
        }

        public override void Execute(AbilityContext context, AbilityRunner runner)
        {
            _coroutine = CoroutineRunner.Instance.StartCoroutine(
                RepeatThenNext(_tickInterval, _duration, context, runner)
            );
        }

        public override bool Cancel(AbilityContext context)
        {
            if(!_isCancellable) return false;
            var snapshot = new List<AbilityRunner>(_subRunners);
            _subRunners.Clear();
            foreach (var subRunner in snapshot)
                subRunner.StopWithCancel();
            return Stop(context);
        }

        public override bool Interrupt(AbilityContext context)
        {
            if(!_isInterruptible) return false;
            var snapshot = new List<AbilityRunner>(_subRunners);
            _subRunners.Clear();
            foreach (var subRunner in snapshot)
                subRunner.StopWithInterrupt();
            return Stop(context);
        }
        private bool Stop(AbilityContext context)
        {
            if (_coroutine != null)
            {
                CoroutineRunner.Instance.StopCoroutine(_coroutine);
                context.Set(ContextKeys.RepeatTickedTimes, _tickedTimes);
                return true;
            }
            return false;
        }
        private IEnumerator RepeatThenNext(float interval, float duration,
            AbilityContext context, AbilityRunner runner)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                AbilityRunner subRunner = new AbilityRunner(_actions, context);
                subRunner.OnCancelled += runner.Cancel;
                subRunner.OnInterrupted += runner.Interrupt;
                subRunner.OnCompleted += () => _subRunners.Remove(subRunner);

                _subRunners.Add(subRunner);
                subRunner.Next();
                _tickedTimes++;
                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }
            runner.Next();
        }
    }
}