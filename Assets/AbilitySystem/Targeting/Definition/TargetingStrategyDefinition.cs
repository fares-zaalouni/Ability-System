using UnityEditor.UIElements;
using UnityEngine;

namespace AbilitySystem.Targeting
{
    public abstract class TargetingStrategyDefinition : ScriptableObject
    {
        [SerializeField] protected LayerMask _targetLayerMask;
        public abstract ITargetingStrategy CreateRuntimeStrategy();
    }
}
