using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem.Core
{
    public class CooldownManager : MonoBehaviour, ICooldownManager
    {
        private static CooldownManager _instance;
        public static CooldownManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AbilitySystem_CooldownManager");
                    _instance = go.AddComponent<CooldownManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        private readonly Dictionary<ICaster, Dictionary<Guid, Cooldown>> _allCooldowns = new();
        private readonly Dictionary<ICaster, Dictionary<Guid, Cooldown>> _activeCooldowns = new();
        private readonly Dictionary<ICaster, Dictionary<Guid, Action>> _cooldownHandlers = new();

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
        public void StartCooldown(ICaster caster, Guid abilityCooldownId)
        {
            Debug.Log("CooldownManager: Starting cooldown for caster " + caster + " with cooldown ID " + abilityCooldownId);
            if(!_allCooldowns.ContainsKey(caster))
            {
                Debug.LogError($"CooldownManager: Caster {caster} not registered. Cannot start cooldown.");
                return;
            }
            var casterCooldowns = _allCooldowns[caster];

            if (!casterCooldowns.ContainsKey(abilityCooldownId))
            {
                Debug.LogError($"CooldownManager: Cooldown {abilityCooldownId} not found for caster {caster}.");
                return;
            }
            var cooldown = casterCooldowns[abilityCooldownId];
            _activeCooldowns[caster][abilityCooldownId] = cooldown;
        }
        
        public void StartGloalCooldown(ICaster caster, float globalCooldownDuration)
        {
            if(!_allCooldowns.TryGetValue(caster, out var casterCooldowns))
            {
                Debug.LogError($"CooldownManager: Caster {caster} not registered. Cannot start global cooldown.");
                return;
            }
            foreach (var cooldown in casterCooldowns.Values)
            {
                cooldown.ForceCooldownIfLonger(globalCooldownDuration);
            }
        }
        public void TickCooldowns(float deltaTime)
        {
            foreach (var casterCooldowns in _activeCooldowns.Values)
            {
                foreach (var cooldown in casterCooldowns.Values)
                {
                    cooldown.Tick(deltaTime);
                }
            }
        }

        public void RegisterCooldown(ICaster caster, Guid cooldownId, Cooldown cooldown)
        {
            if (!_allCooldowns.ContainsKey(caster))
            {
                _allCooldowns[caster] = new Dictionary<Guid, Cooldown>();
                _activeCooldowns[caster] = new Dictionary<Guid, Cooldown>();
                _cooldownHandlers[caster] = new Dictionary<Guid, Action>();
            }
            _allCooldowns[caster][cooldownId] = cooldown;
            var handler = new Action(() => _activeCooldowns[caster].Remove(cooldownId));
            _cooldownHandlers[caster][cooldownId] = handler;
            cooldown.OnCooldownEnded += handler;
        }

        public void UnregisterCooldown(ICaster caster, Guid cooldownId)
        {
            if (_allCooldowns.ContainsKey(caster))
            {
                if(_allCooldowns[caster].TryGetValue(cooldownId, out var cooldown))
                {
                    cooldown.OnCooldownEnded -= _cooldownHandlers[caster][cooldownId];
                    _allCooldowns[caster].Remove(cooldownId);
                    _activeCooldowns[caster].Remove(cooldownId);
                    _cooldownHandlers[caster].Remove(cooldownId);
                }
            }
        }

        public void UnregisterAllCooldowns(ICaster caster)
        {
            if (_allCooldowns.ContainsKey(caster))
            {
                foreach (var kvp in _allCooldowns[caster])
                {
                    kvp.Value.OnCooldownEnded -= _cooldownHandlers[caster][kvp.Key];
                }
                _allCooldowns[caster].Clear();
                _activeCooldowns[caster].Clear();
                _cooldownHandlers[caster].Clear();
            }
        }

        void Update()
        {
            TickCooldowns(Time.deltaTime);
        }
    }
}