using System.Collections.Generic;

public interface ITargetingStrategy
{
    public  List<IAbilityTarget> GetTargets(AbilityContext context);
}