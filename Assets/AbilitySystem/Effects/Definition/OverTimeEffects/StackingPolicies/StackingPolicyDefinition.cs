using UnityEngine;

namespace AbilitySystem.Effects
{
    
    public enum StackingBehavior
    {
        StackAll,
        StackNewest,
        StackOldest,
        None
    }
    
    public abstract class StackingPolicyDefinition : ScriptableObject
    {
        [SerializeField] protected bool _newInstance;
        [SerializeField] protected bool _stackIfSameSource;
        [SerializeField] protected StackingBehavior _stackingBehavior;
        abstract public IStackingPolicy CreateRuntimeStackingPolicy();
        
    }
}