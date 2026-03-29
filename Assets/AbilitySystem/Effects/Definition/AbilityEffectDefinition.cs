using UnityEngine;
using AbilitySystem.Core;
using System;
using System.Collections.Generic;
using AbilitySystem.Attributes;

namespace AbilitySystem.Effects
{
    public abstract class AbilityEffectDefinition : ScriptableObject
    {
        [SerializeField] protected List<ModifierDefinition> _modifiers = new List<ModifierDefinition>();
        public abstract IAbilityEffect CreateEffect(AbilityContext context);
        public int Id => GetInstanceID();
    }
}
