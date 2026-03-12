using System;
using System.Collections.Generic;

namespace AbilitySystem.Core
{
    public static class SignalBus
    {
        private static Dictionary<SignalDefinition, List<Action<AbilityContext>>> _subscribers = new Dictionary<SignalDefinition, List<Action<AbilityContext>>>();

        public static void Subscribe(SignalDefinition signal, Action<AbilityContext> callback)
        {
            if (!_subscribers.ContainsKey(signal))
            {
                _subscribers[signal] = new List<Action<AbilityContext>>();
            }
            _subscribers[signal].Add(callback);
        }

        public static void Unsubscribe(SignalDefinition signal, Action<AbilityContext> callback)
        {
            if (_subscribers.ContainsKey(signal))
            {
                _subscribers[signal].Remove(callback);
            }
        }
        public static void UnsubscribeAll(SignalDefinition signal)
        {
            if (_subscribers.ContainsKey(signal))
            {
                _subscribers[signal].Clear();
            }
        }

        public static void Raise(SignalDefinition signal, AbilityContext context)
        {
            if (_subscribers.ContainsKey(signal))
            {
                foreach (var callback in _subscribers[signal])
                {
                    callback.Invoke(context);
                }
            }
        }
    }
}