using System.Collections.Generic;

public class AbilityRunner
{
    private List<IAbilityAction> _actions;
    private int _currentActionIndex;
    private AbilityContext _context;
    public AbilityRunner(List<IAbilityAction> actions, AbilityContext context)
    {
        _actions = actions;
        _currentActionIndex = -1;
        _context = context;
    }
    public virtual void Next()
    {
        _currentActionIndex++;
        if (_currentActionIndex < _actions.Count)
        {
            _actions[_currentActionIndex].Execute(_context, this);
        }
    }
}