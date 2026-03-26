namespace AbilitySystem.Attributes
{
    public interface IConsumableAttribute
    {
        public float CurrentAmount {get;}
        public string Name { get; }
        public void Consume(float amount);
        public bool CanConsume(float amount);
        public void Add(float amount);
    }
}