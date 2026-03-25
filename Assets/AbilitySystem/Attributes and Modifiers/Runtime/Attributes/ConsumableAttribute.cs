namespace AbilitySystem.Attributes
{
    public class ConsumableAttribute : Attribute, IConsumableAttribute
    {
        public ConsumableAttribute(
            string name,
            float initialAmount, 
            float maxAmount): base( initialAmount, maxAmount, name)
        {
        }


        public float MaxAmount => _runtimeMax;
        public float CurrentAmount => _runtime;

        public void Consume(float amount)
        {
            if (CanConsume(amount))
            {
                _runtime -= amount;
            }
        }

        public bool CanConsume(float amount)
        {
            return CurrentAmount - amount >= 0;
        }
    }
}