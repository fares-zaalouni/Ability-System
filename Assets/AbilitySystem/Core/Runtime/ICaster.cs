using System.Collections.Generic;
using AbilitySystem.Resources;

namespace AbilitySystem.Core
{
    public interface IResourceBearer
    {
        public bool CanConsumeCost(IReadOnlyCollection<Cost> costs);
        public void ConsumeCost(IReadOnlyCollection<Cost> costs);
        public bool TryGetResource(string resourceName, out IResource resource);
        public void RegisterResources();
    }
}