using System.Collections.Generic;
using AbilitySystem.Resources;

namespace AbilitySystem.Core
{
    public interface ICaster
    {
        public bool CanConsumeCost(IReadOnlyCollection<Cost> costs);
        public void ConsumeCost(IReadOnlyCollection<Cost> costs);
        public bool TryGetResource(string resourceName, out IResource resource);
    }
}