using System;
using System.Collections.Generic;

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
    // existing
    public void Next() => GoTo(_currentActionIndex + 1);
    // new
    public void GoTo(int index)
    {
        _currentActionIndex = index;
        if (_currentActionIndex < _actions.Count)
            _actions[_currentActionIndex].Execute(_context, this);
    }
}