using System;
using System.Collections.Generic;

namespace AbilitySystem.Core
{
    public class AbilityRunner
    {
        private List<IAbilityAction> _actions;
        private int _currentActionIndex;
        private AbilityContext _context;
        public event Action OnComplete;
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
                _actions[_currentActionIndex].Execute(_context, this);
            }
            else
            {
                OnComplete?.Invoke();
            }
        }
    }
}