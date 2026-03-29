using System;

namespace AbilitySystem.Attributes
{
    public class ConsumableAttribute : Attribute, IConsumableAttribute
    {
        private float _current;
        public event Action OnCurrentAmountChanged;
        public void OnCurrentAmountChangedInvoke() => OnCurrentAmountChanged?.Invoke();
        public ConsumableAttribute(
            string name,
            float baseValue,
            float initialAmount): base(baseValue,  name)
        {
            _current = initialAmount;
        }

        public float CurrentAmount => _current;

        public void Consume(float amount)
        {
            if (CanConsume(amount))
            {
                _current -= amount;
                if(amount != 0)
                    OnCurrentAmountChangedInvoke();
            }
        }

        public bool CanConsume(float amount)
        {
            return CurrentAmount - amount >= 0;
        }

        public void Add(float amount)
        {
            _current += amount;
            if(_current != _current + amount)
                OnCurrentAmountChangedInvoke();
             if (_current > RuntimeValue)
            {
                _current = RuntimeValue;
            }
        }
    }
}