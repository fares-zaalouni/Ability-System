using System;

namespace AbilitySystem.Core
{
    /// <summary>
    /// A lightweight, per-cast signal instance. Created at runtime by a producer action
    /// (e.g. SpawnProjectileAction) and stored in AbilityContext so that consumer actions
    /// in the same pipeline (e.g. WaitForSignalAction) can subscribe to it without
    /// cross-contaminating other simultaneous casts that use the same SignalDefinition slot.
    /// </summary>
    public class RuntimeSignal
    {
        private event Action<AbilityContext> _raised;

        public void Subscribe(Action<AbilityContext> callback) => _raised += callback;
        public void Unsubscribe(Action<AbilityContext> callback) => _raised -= callback;
        public void Raise(AbilityContext context) => _raised?.Invoke(context);
    }
}
