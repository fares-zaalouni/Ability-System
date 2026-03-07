using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public interface ICaster
{
    public bool CanConsumeCost(IReadOnlyCollection<Cost> costs);
    public void ConsumeCost(IReadOnlyCollection<Cost> costs);
}