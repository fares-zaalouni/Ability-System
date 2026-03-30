using System;
using System.Collections.Generic;

namespace AbilitySystem.Core
{
    using Attribute = Attributes.Attribute;
    public interface IAttributeHolder
    {
        public bool CanConsumeCost(IReadOnlyCollection<Attribute> costs);
        public void ConsumeCost(IReadOnlyCollection<Attribute> costs);
        public bool TryGetAttribute(string attributeName, out Attribute attribute);
        public void RegisterAttributes();        
    }
}