using System.Collections.Generic;
using UnityEngine;
using AbilitySystem.Resources;
using System;

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


        public AbilityInstance(AbilityDefinition definition, IResourceBearer caster)
        {
            _costs = new List<Cost>();
            _casts = new List<AbilityCast>();
            foreach (var costDef in definition.Costs)
            {
                _costs.Add(costDef.CreateRuntimeCost());
            }

            _definition = definition;
            _resourceBearer = caster;
            Cooldown = new Cooldown(definition.Cooldown);
        }

        public bool IsOnCooldown()
        {
            return Cooldown.IsOnCooldown;
        }

        public bool Cast(out WeakReference<AbilityCast> castRef, Blackboard blackboard = null)
        {
            if (!IsOnCooldown() && _resourceBearer.CanConsumeCost(_costs))
            {
                Debug.Log($"Casting {_definition.AbilityName}");
                _resourceBearer.ConsumeCost(_costs);
                CooldownManager.Instance.StartCooldown(_caster, Id);
            
                AbilityCast cast = new AbilityCast(_resourceBearer, _definition, blackboard);
                
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