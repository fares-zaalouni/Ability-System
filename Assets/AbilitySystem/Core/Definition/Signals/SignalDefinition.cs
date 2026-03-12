using UnityEngine;

namespace AbilitySystem.Core
{
    [CreateAssetMenu(fileName = "SignalDefinition", menuName = "Ability System/Signal")]
    public class SignalDefinition : ScriptableObject
    {
        [SerializeField] private string _signalName;

        // Stable per-SO key used to store/retrieve a RuntimeSignal in AbilityContext.
        public string ContextKey => $"__sig_{GetInstanceID()}";
    }
}