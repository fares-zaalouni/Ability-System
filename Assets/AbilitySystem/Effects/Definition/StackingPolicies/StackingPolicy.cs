using UnityEngine;

namespace AbilitySystem.Effects
{
    public enum DurationRefreshPolicy
    {
        Refresh,
        Extend,
        None
    }
    public enum StackingBehavior
    {
        Stack,
        NewInstance,
        None
    }
    [CreateAssetMenu(fileName = "StackingPolicy", menuName = "Ability System/Effects/Lifetime/StackingPolicy")]
    public class StackingPolicy : ScriptableObject
    {
        [SerializeField] private DurationRefreshPolicy _durationRefreshPolicy;
        [SerializeField] private StackingBehavior _stackingBehavior;
        
    }
}