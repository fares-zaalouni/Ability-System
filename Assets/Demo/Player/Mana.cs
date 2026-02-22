public class Mana : IResource
{
    public float ResourceAmount { get; private set; }
    public string ResourceName => "Mana";

    public Mana(float initialAmount)
    {
        ResourceAmount = initialAmount;
    }

    public void Consume(float amount)
    {
        if (CanConsume(amount))
        {
            ResourceAmount -= amount;
        }
    }

    public bool CanConsume(float amount)
    {
        return ResourceAmount >= amount;
    }
}