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
        private IResourceBearer _caster;
        private float _cooldownRemaining;


        public AbilityInstance(AbilityDefinition definition, IResourceBearer caster)
        {
            _costs = new List<Cost>();
            _casts = new List<AbilityCast>();
            foreach (var costDef in definition.Costs)
            {
                _costs.Add(costDef.CreateRuntimeCost());
            }

            _definition = definition;
            _caster = caster;
            _cooldownRemaining = 0f;
        }

        public bool IsOnCooldown()
        {
            return _cooldownRemaining > 0f;
        }

        public bool Cast(out WeakReference<AbilityCast> castRef, Blackboard blackboard = null)
        {
            if (!IsOnCooldown() && _caster.CanConsumeCost(_costs))
            {
                Debug.Log($"Casting {_definition.AbilityName}");
                _caster.ConsumeCost(_costs);
                //_cooldownRemaining = _definition.Cooldown;
            
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