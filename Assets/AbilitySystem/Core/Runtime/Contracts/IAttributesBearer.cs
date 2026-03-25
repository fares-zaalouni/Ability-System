using System.Collections.Generic;
using AbilitySystem.Attributes;

namespace AbilitySystem.Core
{
    public interface IAttibuteBearer
    {
        public bool CanConsumeCost(IReadOnlyCollection<AbilityCost> costs);
        public void ConsumeCost(IReadOnlyCollection<AbilityCost> costs);
        public bool TryGetAttribute(string attributeName, out Attribute attribute);
        public void RegisterAttribute();
    }
}