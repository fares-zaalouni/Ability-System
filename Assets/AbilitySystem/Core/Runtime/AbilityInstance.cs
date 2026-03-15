using System.Collections.Generic;
using UnityEngine;
using AbilitySystem.Resources;
using System;
using NUnit.Framework;

namespace AbilitySystem.Core
{
    public class AbilityInstance
    {
        private AbilityDefinition _definition;
        private List<Cost> _costs;
        private List<AbilityCast> _casts; 
        private IResourceBearer _resourceBearer;
        private ICaster _caster;
        public Cooldown Cooldown { get; private set; }

        public Guid Id { get; } = Guid.NewGuid();


        public AbilityInstance(AbilityDefinition definition, ICaster caster, IResourceBearer resourceBearer = null)
        {
            _costs = new List<Cost>();
            _casts = new List<AbilityCast>();
            foreach (var costDef in definition.Costs)
            {
                _costs.Add(costDef.CreateRuntimeCost());
            }

            _definition = definition;
            _resourceBearer = resourceBearer;
            _caster = caster;
            Cooldown = new Cooldown(definition.Cooldown);
        }

        public bool IsOnCooldown => Cooldown.IsOnCooldown;
        

        public bool Cast(out WeakReference<AbilityCast> castRef, Dictionary<string, object> blackboard = null)
        {
            bool hasCosts = _costs.Count > 0;
            if(_resourceBearer == null && hasCosts)
            {
                Debug.LogError($"AbilityInstance: Caster {_caster} cannot bear resources, but ability {_definition.AbilityName} has costs.");
                castRef = null;
                return false;
            }
            
            bool canPayCosts = !hasCosts || (_resourceBearer != null && _resourceBearer.CanConsumeCost(_costs));
            
            if (!IsOnCooldown && canPayCosts)
            {
                Debug.Log($"Casting {_definition.AbilityName}");
                _resourceBearer.ConsumeCost(_costs);
                CooldownManager.Instance.StartCooldown(_caster, Id);
            
                AbilityCast cast = new AbilityCast(_caster, _definition, blackboard);
                
                _casts.Add(cast);
                cast.Execute();
                castRef = new WeakReference<AbilityCast>(cast);
                return true;
            }
            castRef = null;
            return false;
        }

    }
}