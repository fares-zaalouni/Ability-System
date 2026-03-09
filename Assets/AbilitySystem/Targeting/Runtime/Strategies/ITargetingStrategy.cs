using System.Collections.Generic;
using AbilitySystem.Core;

namespace AbilitySystem.Targeting
{
    public interface ITargetingStrategy
    {
        public  List<IAbilityTarget> GetTargets(AbilityContext context);
    }
}