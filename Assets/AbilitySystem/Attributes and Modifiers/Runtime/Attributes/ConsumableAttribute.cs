namespace AbilitySystem.Attributes
{
    public class ConsumableAttribute : Attribute, IConsumableAttribute
    {
        private float _current;
        public ConsumableAttribute(
            string name,
            float initialAmount): base(initialAmount,  name)
        {
            _current = initialAmount;
        }

        public float CurrentAmount => _current;

        public void Consume(float amount)
        {
            if (CanConsume(amount))
            {
                _current -= amount;
            }
        }

        public bool CanConsume(float amount)
        {
            return CurrentAmount - amount >= 0;
        }

        public void Add(float amount)
        {
            _current += amount;
             if (_current > RuntimeValue)
            {
                _current = RuntimeValue;
            }
        }
    }
}