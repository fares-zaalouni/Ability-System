namespace AbilitySystem.Resources
{
    public class ManaResource : IResource
    {
        public float MaxAmount { get; private set; }
        public float CurrentAmount {get; private set;} 
        public string Name {get; private set; }
        public float RegenAmount { get; private set; }
        
        public ManaResource(string name, float maxAmount, float regenAmount)
        {
            Name = name;
            MaxAmount = maxAmount;
            CurrentAmount = maxAmount;
            RegenAmount = regenAmount;
        }

        public void Consume(float amount)
        {
            if (CanConsume(amount))
            {
                CurrentAmount -= amount;
            }
            else
            {
                throw new System.InvalidOperationException("Not enough mana to consume.");
            }
        }

        public bool CanConsume(float amount)
        {
            return CurrentAmount >= amount;
        }
    }
}