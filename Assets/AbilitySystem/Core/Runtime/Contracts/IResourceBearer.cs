using System.Collections.Generic;
using AbilitySystem.Resources;

namespace AbilitySystem.Core
{
    public interface IResourceBearer
    {
        public bool CanConsumeCost(IReadOnlyCollection<AbilityCost> costs);
        public void ConsumeCost(IReadOnlyCollection<AbilityCost> costs);
        public bool TryGetResource(string resourceName, out IResource resource);
        public void RegisterResources();
    }
}