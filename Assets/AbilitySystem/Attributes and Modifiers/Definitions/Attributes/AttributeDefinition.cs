using UnityEngine;

namespace AbilitySystem.Attributes
{
    public abstract class AttributeDefinition : ScriptableObject
    {
        [SerializeField] protected string _attributeName;
        [SerializeField] protected float _initialAmount;
        [SerializeField, HideInInspector] private string _id;
        public string AttributeType => _id;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
                _id = System.Guid.NewGuid().ToString();
        }
        public virtual Attribute CreateRuntimeAttribute()
        {
            return new Attribute(_initialAmount, _attributeName != "" ? _attributeName : "-No Name-");
        }
    }
}