using System.Collections.Generic;

public class ConditionalAction : IAbilityAction
{
    private readonly ConditionDefinition _condition;
    private readonly List<IAbilityAction> _trueActions;
    private readonly List<IAbilityAction> _falseActions;

    public ConditionalAction(ConditionDefinition condition, List<IAbilityAction> trueActions, List<IAbilityAction> falseActions)
    {
        _condition = condition;
        _trueActions = trueActions;
        _falseActions = falseActions;
    }

    public void Execute(AbilityContext context, AbilityRunner runner)
    {
        var branch = _condition.Evaluate(context) ? _trueActions : _falseActions;
        var subRunner = new AbilityRunner(branch, context);
        subRunner.OnComplete += runner.Next;
        subRunner.Next();
    }
}