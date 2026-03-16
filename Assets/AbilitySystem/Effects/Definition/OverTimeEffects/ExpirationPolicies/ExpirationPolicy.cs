using UnityEngine;

namespace AbilitySystem.Effects
{
    public enum EffectExpirationPolicy
    {
        RemoveStack,
        RemoveAllStacks,
        None
    }
    public enum DurationPolicy
    {
        Refresh,
        None
    }
    [CreateAssetMenu(fileName = "ExpirationPolicy", menuName = "Ability System/Effects/Lifetime/ExpirationPolicy")]
    public class ExpirationPolicy : ScriptableObject
    {
        [SerializeField] private EffectExpirationPolicy _effectExpirationPolicy;
        [SerializeField] private DurationPolicy _durationPolicy;
    }
}