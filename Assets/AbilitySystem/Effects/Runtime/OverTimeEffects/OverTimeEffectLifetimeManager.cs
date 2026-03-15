using System;
using System.Collections.Generic;
using AbilitySystem.Targeting;
using UnityEngine;

namespace AbilitySystem.Effects
{
    public class OverTimeEffectLifetimeManager : MonoBehaviour
    {
        private static OverTimeEffectLifetimeManager _instance;
        public static OverTimeEffectLifetimeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AbilitySystem_OverTimeEffectLifetimeManager");
                    _instance = go.AddComponent<OverTimeEffectLifetimeManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        private readonly Dictionary<IAbilityTarget, Dictionary<int, List<OverTimeEffect>>> _activeOverTimeEffects = new();
        private readonly Dictionary<IAbilityTarget, Dictionary<int, Action>> _overTimeEffectHandlers = new();
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterOverTimeEffect(IAbilityTarget target, OverTimeEffect effect)
        {
            if (!_activeOverTimeEffects.ContainsKey(target))
            {
                _activeOverTimeEffects[target] = new Dictionary<int, List<OverTimeEffect>>();
                _overTimeEffectHandlers[target] = new Dictionary<int, Action>();
            }
            if (!_activeOverTimeEffects[target].ContainsKey(effect.Id))
            {
                _activeOverTimeEffects[target][effect.Id] = new List<OverTimeEffect>();
            }

            effect.HandleStacking(target, _activeOverTimeEffects[target][effect.Id]);

            var handler = new Action(() =>
            {
                if(_activeOverTimeEffects.TryGetValue(target, out var targetEffects))
                {
                    if(targetEffects.TryGetValue(effect.Id, out var activeEffect))
                    {
                        effect.HandleExpiration(target, activeEffect);
                        if(activeEffect.Count == 0)
                        {
                            targetEffects.Remove(effect.Id);
                        }
                    }
                }else
                {
                    Debug.LogError($"Effect Action should not be called when target {target} has no active effects.");
                }
            });
            effect.EffectExpired += handler;
            _overTimeEffectHandlers[target][effect.Id] = handler;
        }

        public void UnregisterOverTimeEffect(IAbilityTarget target, OverTimeEffect effect)
        {
            if (_activeOverTimeEffects.TryGetValue(target, out var targetEffects) && targetEffects.TryGetValue(effect.Id, out var activeEffect))
            {
                activeEffect.Remove(effect);
                if (activeEffect.Count == 0)
                {
                    targetEffects.Remove(effect.Id);
                }
            }
            else
            {
                Debug.LogError($"Attempting to unregister effect {effect} from target {target}, but it was not found in active effects.");
            }

            if (_overTimeEffectHandlers.TryGetValue(target, out var targetHandlers) && targetHandlers.TryGetValue(effect.Id, out var handler))
            {
                effect.EffectExpired -= handler;
                targetHandlers.Remove(effect.Id);
            }
            else
            {
                Debug.LogError($"Attempting to unregister effect handler for effect {effect} from target {target}, but it was not found in handlers.");
            }
        }

    }
}