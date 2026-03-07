using System.Collections.Generic;

public class TargetingAction : IAbilityAction
{
    private ITargetingStrategy _targetingStrategy;

    public TargetingAction(ITargetingStrategy targetingStrategy)
    {
        _targetingStrategy = targetingStrategy;
    }

    public void Execute(AbilityContext context, AbilityRunner runner)
    {
        List<IAbilityTarget> targets = _targetingStrategy.GetTargets(context);
        context.SetTargets(targets);
        runner.Next();
    }
}