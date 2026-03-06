using System.Collections.Generic;

public struct AbilityContext
{
    public ICaster Caster { get; private set; }
    public List<IAbilityTarget> Targets { get; private set; }

    public AbilityContext(ICaster caster, List<IAbilityTarget> targets)
    {
        Caster = caster;
        Targets = targets;
    }
}