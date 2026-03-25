using System.Collections.Generic;
using System.Linq;

namespace AbilitySystem.Attributes
{
    public class Attribute
    {
        public string Name { get; protected set; }

        protected float _base;
        protected float _baseMax;
        protected float _runtime;
        protected float _runtimeMax;

        public float BaseValue => _base;
        public float BaseMaxValue => _baseMax;
        public float CalculatedValue => _runtime;
        public float CalculatedMaxValue => _runtimeMax;

        private List<IModifier> _modifiers = new List<IModifier>();
        public Attribute(float baseValue, float baseMaxValue, string name = "-No Name-")
        {
            Name = name;
            _base = baseValue;
            _baseMax = baseMaxValue;
            _runtime = baseValue;
            _runtimeMax = baseMaxValue;
        }

        public void SetBaseValue(float newValue)
        {
            _base = newValue;
            RecalculateRuntimeValues();
        }

        public void SetBaseMaxValue(float newValue)
        {
            _baseMax = newValue;
            RecalculateRuntimeValues();
        }

        public void SetBaseValues(float newValue, float newMaxValue)
        {
            _base = newValue;
            _baseMax = newMaxValue;
            RecalculateRuntimeValues();
        }

        public void IncreaseBaseValue(float amount)
        {
            _base += amount;
            RecalculateRuntimeValues();
        }

        public void IncreaseBaseMaxValue(float amount)
        {
            _baseMax += amount;
            RecalculateRuntimeValues();
        }

        public void IncreaseBaseValues(float amount, float maxAmount)
        {
            _base += amount;
            _baseMax += maxAmount;
            RecalculateRuntimeValues();
        }

        public void DecreaseBaseValue(float amount)
        {
            _base -= amount;
            RecalculateRuntimeValues();
        }

        public void DecreaseBaseMaxValue(float amount)
        {
            _baseMax -= amount;
            RecalculateRuntimeValues();
        }

        public void DecreaseBaseValues(float amount, float maxAmount)
        {
            _base -= amount;
            _baseMax -= maxAmount;
            RecalculateRuntimeValues();
        }
        
        public void AddModifier(IModifier modifier)
        {
            _modifiers.Add(modifier);
            _modifiers = _modifiers.OrderBy(m => m.Priority).ToList();
            RecalculateRuntimeValues();
        }

        public void RemoveModifier(IModifier modifier)
        {
            if (_modifiers.Remove(modifier))
            {
                _modifiers = _modifiers.OrderBy(m => m.Priority).ToList();
                RecalculateRuntimeValues();
            }
        }

        private void RecalculateRuntimeValues()
        {
            _runtime = _base;
            _runtimeMax = _baseMax;
            foreach (var modifier in _modifiers)
            {
                modifier.Apply(this);
            }
        }
        
        public float GetBonus()
        {
            return _runtime - _base;
        }
    }
}