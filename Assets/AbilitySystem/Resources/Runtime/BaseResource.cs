
using UnityEngine;

namespace AbilitySystem.Resources
{
    public class BaseResource : IResource
    {
        public float MaxAmount { get; private set; }
        public float CurrentAmount {get; private set;} 
        public string Name {get; private set; }
        public float RegenAmount { get; private set; }
        
        public BaseResource(string name, float maxAmount, float regenAmount)
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
                //change later
                Debug.Log($"Not enough {Name} to consume. Required: {amount}, Available: {CurrentAmount}");
            }
        }

        public bool CanConsume(float amount)
        {
            return CurrentAmount >= amount;
        }
    }
}