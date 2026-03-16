using UnityEngine;

namespace AbilitySystem.Effects
{
    public enum DurationRefreshPolicy
    {
        RefreshAll,
        RefreshNewest,
        RefreshOldest,
        ExtendAll,
        ExtendNewest,
        ExtendOldest,
        None
    }
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
        [SerializeField] protected DurationRefreshPolicy _durationRefreshPolicy;
        [SerializeField] protected StackingBehavior _stackingBehavior;
        abstract public IStackingPolicy CreateRuntimeStackingStrategy();
        
    }
}