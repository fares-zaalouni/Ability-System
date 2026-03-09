using System.Collections.Generic;


public interface ICaster
{
    public bool CanConsumeCost(IReadOnlyCollection<Cost> costs);
    public void ConsumeCost(IReadOnlyCollection<Cost> costs);
}