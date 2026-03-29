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

        private readonly Dictionary<IAbilityTarget, Dictionary<int, OverTimeEffectGroup>> _activeOverTimeEffects = new();
        private readonly Dictionary<IAbilityTarget, Dictionary<Guid, Action>> _overTimeEffectHandlers = new();


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

        public void ReApplyOverTimeEffectsModifier(IAbilityTarget target, int effectTypeId = -1)
        {
            if (_activeOverTimeEffects.TryGetValue(target, out var effectsById))
            {
                if (effectTypeId == -1)
                {
                    foreach (var group in effectsById.Values)
                    {
                        group.ReApplyModifiers(target);
                    }
                }
                else if (effectsById.TryGetValue(effectTypeId, out var group))
                {
                    group.ReApplyModifiers(target);
                }
            }
        }

        public void RegisterOverTimeEffect(IAbilityTarget target, OverTimeEffect effect)
        {
            if (!_activeOverTimeEffects.ContainsKey(target))
                _activeOverTimeEffects[target] = new Dictionary<int, OverTimeEffectGroup>();
            if (!_activeOverTimeEffects[target].ContainsKey(effect.EffectTypeId))
                _activeOverTimeEffects[target][effect.EffectTypeId] = new OverTimeEffectGroup();

            if (effect.StackingPolicy.HandleStacking(target, effect, _activeOverTimeEffects[target][effect.EffectTypeId]))
            {
                Action handler = () =>
                {
                    UnregisterOverTimeEffect(target, effect);
                };
                effect.EffectExpired += handler;

                if (!_overTimeEffectHandlers.ContainsKey(target))
                    _overTimeEffectHandlers[target] = new Dictionary<Guid, Action>();
                _overTimeEffectHandlers[target][effect.Id] = handler;      
            }
            if(_activeOverTimeEffects[target][effect.EffectTypeId].EffectCount == 0)
                    Debug.LogWarning("Registering first OverTimeEffect of type " + effect.EffectTypeId +" but it was not added to the group. Check if the stacking policy is set up correctly. Effect: " + effect);
        }

        public void UnregisterOverTimeEffect(IAbilityTarget target, OverTimeEffect effect)
        {
            if (_activeOverTimeEffects.TryGetValue(target, out var effectsById) && effectsById.TryGetValue(effect.EffectTypeId, out var group))
            {
                group.RemoveEffect(effect);
            }
            if (_overTimeEffectHandlers.TryGetValue(target, out var handlersById) && handlersById.TryGetValue(effect.Id, out var handler))
            {
                effect.EffectExpired -= handler;
                handlersById.Remove(effect.Id);
            }
        }

        private void Tick(float deltaTime)
        {
            foreach (var targetEntry in _activeOverTimeEffects)
            {
                var target = targetEntry.Key;
                foreach (var groupEntry in targetEntry.Value)
                {
                    var group = groupEntry.Value;
                    group.TickAll(deltaTime, target);
                }
            }
        }

        public void FixedUpdate()
        {
            Tick(Time.fixedDeltaTime);
        }
        public void CleanUpTarget(IAbilityTarget target)
        {
            if (_activeOverTimeEffects.TryGetValue(target, out var effectsById))
            {
                foreach (var group in effectsById.Values)
                {
                    for (int i = group.Effects.Count - 1; i >= 0; i--)
                    {
                        UnregisterOverTimeEffect(target, group.Effects[i]);
                    }
                }
                _activeOverTimeEffects.Remove(target);
            }
        }
        public void CleanUpTargetEffectType(IAbilityTarget target, int effectTypeId)
        {
            if (_activeOverTimeEffects.TryGetValue(target, out var effectsById) && effectsById.TryGetValue(effectTypeId, out var group))
            {
                for (int i = group.Effects.Count - 1; i >= 0; i--)
                {
                    UnregisterOverTimeEffect(target, group.Effects[i]);
                }
                effectsById.Remove(effectTypeId);
            }
        }
    }
}