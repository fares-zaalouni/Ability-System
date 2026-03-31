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
    public enum RefreshPolicy
    {
        RefreshIfSameSource,
        AlwaysRefresh,
        NeverRefresh
    }
    [CreateAssetMenu(fileName = "BasicDurationPolicyDefinition", menuName = "Ability System/Effects/Duration Policies/Basic Duration Policy")]
    public class BasicDurationPolicyDefinition : DurationPolicyDefinition
    {
        [SerializeField] protected RefreshPolicy _refreshPolicy;
        [SerializeField] protected DurationRefreshPolicy _durationRefreshPolicy;

        public override IDurationPolicy CreateRuntimeDurationPolicy()
        {
            return new BasicDurationPolicy(_durationRefreshPolicy, _refreshPolicy);
        }
    }
}