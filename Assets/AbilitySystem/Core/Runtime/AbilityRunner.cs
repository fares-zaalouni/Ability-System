using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem.Core
{
    public class AbilityRunner
    {
        private List<IAbilityAction> _actions;
        private int _currentActionIndex;
        private AbilityContext _context;
        public event Action OnCompleted;
        public event Action OnInterrupted;
        public event Action OnCancelled;
        public AbilityRunner(List<IAbilityAction> actions, AbilityContext context)
        {
            _actions = actions;
            _currentActionIndex = -1;
            _context = context;
        }
        public void Next()
        {
            _currentActionIndex++;
            if (_currentActionIndex < _actions.Count)
            {
                Debug.Log("Executing action: " + _actions[_currentActionIndex].GetType().Name);
                _actions[_currentActionIndex].Execute(_context, this);
            }
            else
            {
                OnCompleted?.Invoke();
            }
        }

        public void Cancel()
        {
            if (_currentActionIndex < _actions.Count && _actions[_currentActionIndex] is SustainedAction cancellable)
            {
                if (cancellable.Cancel(_context))
                    TakeAftermathAction(cancellable.CancelAftermath);
            }
        }
        public void Interrupt()
        {
            if (_currentActionIndex < _actions.Count && _actions[_currentActionIndex] is SustainedAction interruptable)
            {
                if (interruptable.Interrupt(_context))
                    TakeAftermathAction(interruptable.InterruptAftermath);
            }
        }

        // External stop variants — halt the current action without firing any events.
        // Use these when a parent action owns this runner and handles propagation itself.
        // Unlike Cancel()/Interrupt(), these never call TakeAftermathAction, so OnCancelled
        // and OnInterrupted are never raised and cannot re-enter the caller.
        public void StopWithCancel()
        {
            if (_currentActionIndex < _actions.Count && _actions[_currentActionIndex] is SustainedAction sa)
                sa.Cancel(_context);
        }

        public void StopWithInterrupt()
        {
            if (_currentActionIndex < _actions.Count && _actions[_currentActionIndex] is SustainedAction sa)
                sa.Interrupt(_context);
        }

        private void TakeAftermathAction(SustainedActionEndAftermath aftermath)
        {
            switch (aftermath)
            {
                case SustainedActionEndAftermath.None: Next(); break;
                case SustainedActionEndAftermath.Cancel: OnCancelled?.Invoke(); break;
                case SustainedActionEndAftermath.Interrupt: OnInterrupted?.Invoke(); break;
            }
        }
    }
}