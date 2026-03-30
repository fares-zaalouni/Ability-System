using System;
using System.Collections.Generic;
using AbilitySystem.Targeting;
using AbilitySystem.Utility;
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
        private readonly List<int> _scratchEmptyEffectTypeIds = new();
        private readonly List<IAbilityTarget> _scratchTargetsToPrune = new();
        private readonly List<IAbilityTarget> _scratchHandlerTargetsToRemove = new();
        [SerializeField] private float _pruneIntervalSeconds = 0.5f;
        private float _timeSinceLastPrune;


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
                    AbilityDebug.LogWarning("Registering first OverTimeEffect of type " + effect.EffectTypeId +" but it was not added to the group. Check if the stacking policy is set up correctly. Effect: " + effect);
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

            PruneTargetEntries(target);
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

            _timeSinceLastPrune += Time.fixedDeltaTime;
            if (_timeSinceLastPrune >= _pruneIntervalSeconds)
            {
                PruneEmptyEntries();
                _timeSinceLastPrune = 0f;
            }
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

            PruneTargetEntries(target);
        }

        private void PruneTargetEntries(IAbilityTarget target)
        {
            if (_activeOverTimeEffects.TryGetValue(target, out var effectsById))
            {
                _scratchEmptyEffectTypeIds.Clear();
                foreach (var kvp in effectsById)
                {
                    if (kvp.Value == null || kvp.Value.EffectCount == 0)
                    {
                        _scratchEmptyEffectTypeIds.Add(kvp.Key);
                    }
                }

                foreach (var effectTypeId in _scratchEmptyEffectTypeIds)
                {
                    effectsById.Remove(effectTypeId);
                }

                if (effectsById.Count == 0)
                {
                    _activeOverTimeEffects.Remove(target);
                }
            }

            if (_overTimeEffectHandlers.TryGetValue(target, out var handlersById) && (handlersById == null || handlersById.Count == 0))
            {
                _overTimeEffectHandlers.Remove(target);
            }
        }

        private void PruneEmptyEntries()
        {
            _scratchTargetsToPrune.Clear();
            foreach (var target in _activeOverTimeEffects.Keys)
            {
                _scratchTargetsToPrune.Add(target);
            }

            foreach (var target in _scratchTargetsToPrune)
            {
                PruneTargetEntries(target);
            }

            _scratchHandlerTargetsToRemove.Clear();
            foreach (var kvp in _overTimeEffectHandlers)
            {
                if (kvp.Value == null || kvp.Value.Count == 0)
                {
                    _scratchHandlerTargetsToRemove.Add(kvp.Key);
                }
            }

            foreach (var target in _scratchHandlerTargetsToRemove)
            {
                _overTimeEffectHandlers.Remove(target);
            }
        }
    }
}