using System.Collections.Generic;
using AbilitySystem.Attributes;

namespace AbilitySystem.Core
{
    public interface IAttributeHolder
    {
        public bool CanConsumeCost(IReadOnlyCollection<Attribute> costs);
        public void ConsumeCost(IReadOnlyCollection<Attribute> costs);
        public bool TryGetAttribute(string attributeName, out Attribute attribute);
        public void RegisterAttributes();
    }
}