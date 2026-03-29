using System.Collections.Generic;
using UnityEngine;
using System;
using AbilitySystem.Utility;
using AbilitySystem.Attributes;

namespace AbilitySystem.Core
{
    using Attribute = AbilitySystem.Attributes.Attribute;
    public class AbilityInstance
    {
        private AbilityDefinition _definition;
        public AbilityDefinition Definition => _definition;
        private List<Attribute> _costs;
        private List<AbilityCast> _casts; 
        private Dictionary<AbilityCast, Action<AbilityContext>> _castCompletionCallbacks = new Dictionary<AbilityCast, Action<AbilityContext>>();   
        private IAttributeHolder _resourceBearer;
        private ICaster _caster;
        public Cooldown Cooldown { get; private set; }
        public Guid Id { get; } = Guid.NewGuid();


        public AbilityInstance(AbilityDefinition definition, ICaster caster, IAttributeHolder resourceBearer = null)
        {
            if (definition == null)
            {
                AbilityDebug.LogError($"AbilityInstance: AbilityDefinition is null for caster {caster}.");
                return;
            }
            if(definition.Costs == null)
            {
                AbilityDebug.LogError($"AbilityInstance: Costs list is null in AbilityDefinition {definition.AbilityName}.");
                definition.Costs = new List<AttributeDefinition>();
            }    
            _costs = new List<Attribute>();
            _casts = new List<AbilityCast>();

            foreach (var costDef in definition.Costs)
            {
                if( costDef == null)
                {
                    AbilityDebug.LogError($"AbilityInstance: AbilityDefinition {definition.AbilityName} has a null cost definition.");
                    continue;
                }

                _costs.Add(costDef.CreateRuntimeAttribute());
            }

            _definition = definition;
            _resourceBearer = resourceBearer;
            _caster = caster;
            Cooldown = new Cooldown(definition.Cooldown);
        }

        public bool IsOnCooldown => Cooldown.IsOnCooldown;
        

        public bool Cast(out WeakReference<AbilityCast> castRef, IEnumerable<object> initialDependencies = null)
        {
            bool hasCosts = _costs.Count > 0;
            if(_resourceBearer == null && hasCosts)
            {
                AbilityDebug.LogError($"AbilityInstance: Caster {_caster} cannot bear resources, but ability {_definition.AbilityName} has costs.");
                castRef = null;
                return false;
            }
            
            bool canPayCosts = !hasCosts || (_resourceBearer != null && _resourceBearer.CanConsumeCost(_costs));
            
            if (!IsOnCooldown && canPayCosts)
            {
                Debug.Log($"Casting {_definition.AbilityName}");
                _resourceBearer.ConsumeCost(_costs);
                CooldownManager.Instance.StartCooldown(_caster, Id);
                Action<AbilityContext> callback = null;
                AbilityCast cast = new AbilityCast(_caster, _definition, initialDependencies);
                _castCompletionCallbacks[cast] = callback;
                Action<AbilityContext> completionCallback = (ctx) =>
                {
                    callback?.Invoke(ctx);
                    _castCompletionCallbacks.Remove(cast);
                };

                cast.OnCancelled += completionCallback;
                cast.OnCompleted += completionCallback;
                cast.OnInterrupted += completionCallback;
                _casts.Add(cast);
                cast.Execute();
                castRef = new WeakReference<AbilityCast>(cast);
                return true;
            }
            castRef = null;
            return false;
        }

        public void Dispose()
        {
            CooldownManager.Instance.UnregisterCooldown(_caster, Id);
            foreach(var (key, callback) in _castCompletionCallbacks)
            {
                key.OnCancelled -= callback;
                key.OnCompleted -= callback;
                key.OnInterrupted -= callback;
            }
            foreach (var cast in _casts)
            {
                cast.Cancel();
            }
            _casts.Clear();
        }

    }
}