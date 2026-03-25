using UnityEngine;

namespace AbilitySystem.Attributes
{
    public abstract class AttributeDefinition : ScriptableObject
    {
        [SerializeField] protected string _attributeName;
        [SerializeField] protected float _maxAmount;
        [SerializeField] protected float _initialAmount;
        public abstract string AttributeType { get; }
        public string Id => GetInstanceID().ToString();
        public virtual Attribute CreateRuntimeResource()
        {
            return new Attribute(_initialAmount, _maxAmount,  _attributeName !=""? _attributeName : "-No Name-");
        }
    }
}