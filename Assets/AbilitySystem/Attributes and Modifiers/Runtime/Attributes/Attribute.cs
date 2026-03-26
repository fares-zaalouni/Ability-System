using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbilitySystem.Attributes
{
    public class Attribute
    {
        public string Name { get; protected set; }

        protected float _base;
        protected float _runtime;

        public float BaseValue => _base;
        public float RuntimeValue => _runtime;

        private List<IModifier> _modifiers = new List<IModifier>();
        public Attribute(float baseValue, string name = "-No Name-")
        {
            Name = name;
            _base = baseValue;
            _runtime = baseValue;
        }

        public void SetBaseValue(float newValue)
        {
            _base = newValue;
            RecalculateRuntimeValues();
        }

        public void IncreaseBaseValue(float amount)
        {
            _base += amount;
            RecalculateRuntimeValues();
        }

        public void DecreaseBaseValue(float amount)
        {
            _base -= amount;
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
            foreach (var modifier in _modifiers)
            {
                modifier.Apply(this);
            }
        }
        
        public float GetBonus()
        {
            return _runtime - _base;
        }

        public void AddToRuntime(float amount)
        {
            _runtime += amount;
        }
        public void SubtractFromRuntime(float amount)
        {
            _runtime -= amount;
        }

    }
}