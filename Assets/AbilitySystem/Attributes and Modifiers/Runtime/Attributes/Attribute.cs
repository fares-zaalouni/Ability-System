using System;
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

        public event Action OnRuntimeValueChanged;
        public void OnRuntimeValueChangedInvoke()
        {
            OnRuntimeValueChanged?.Invoke();
        }
        public event Action OnBaseValueChanged;
        public void OnBaseValueChangedInvoke()
        {
            OnBaseValueChanged?.Invoke();
        }

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
            OnBaseValueChangedInvoke();
            RecalculateRuntimeValues();
        }

        public void IncreaseBaseValue(float amount)
        {
            _base += amount;
            OnBaseValueChangedInvoke();
            RecalculateRuntimeValues();
        }

        public void DecreaseBaseValue(float amount)
        {
            _base -= amount;
            OnBaseValueChangedInvoke();
            RecalculateRuntimeValues();
        }
        
        public void AddModifier(IModifier modifier)
        {
            _modifiers.Add(modifier);
            _modifiers = _modifiers.OrderBy(m => m.Priority).ToList();
            RecalculateRuntimeValues();
        }

        public void AddModifiers(IEnumerable<IModifier> modifiers)
        {
            _modifiers.AddRange(modifiers);
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

        public void RemoveModifiers(IEnumerable<IModifier> modifiers)
        {
            bool removedAny = false;
            foreach (var modifier in modifiers)
            {
                if (_modifiers.Remove(modifier))
                {
                    removedAny = true;
                }
            }
            if (removedAny)
            {
                _modifiers = _modifiers.OrderBy(m => m.Priority).ToList();
                RecalculateRuntimeValues();
            }
        }

        public void ClearModifiers()
        {
            if (_modifiers.Count > 0)
            {
                _modifiers.Clear();
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
        public void SetRuntime(float newValue)
        {
            _runtime = newValue;
            if(_runtime != newValue)
                OnRuntimeValueChangedInvoke();
        }
        public void AddToRuntime(float amount)
        {
            _runtime += amount;
            if(amount != 0)
                OnRuntimeValueChangedInvoke();
        }
        public void SubtractFromRuntime(float amount)
        {
            _runtime -= amount;
            if(amount != 0)
                OnRuntimeValueChangedInvoke();
        }

    }
}