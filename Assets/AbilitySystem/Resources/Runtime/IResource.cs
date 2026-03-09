namespace AbilitySystem.Resources
{
    public interface IResource
    {
        public float MaxAmount { get; }
        public float CurrentAmount {get;}
        public string Name { get; }
        public void Consume(float amount);
        public bool CanConsume(float amount);

    }
}