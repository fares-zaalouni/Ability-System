using AbilitySystem.Core;
using UnityEngine;

namespace AbilitySystem.Effects
{
    public abstract class OverTimeEffectDefinition : AbilityEffectDefinition
    {
        [SerializeField] protected float _duration;
        [SerializeField] protected float _tickInterval;
        [SerializeField] protected int _maxStacks;
        [SerializeField] protected int _initialStacks = 1;
        [SerializeField] protected bool _applyOnce;
        [SerializeField] protected StackingPolicyDefinition _stackingPolicy;

        public StackingPolicyDefinition StackingPolicy => _stackingPolicy;
    }
    
}