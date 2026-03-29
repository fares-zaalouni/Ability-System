using UnityEngine;

namespace AbilitySystem.Attributes
{
    [CreateAssetMenu(fileName = "Attribute", menuName = "Ability System/Attributes/Attribute")]
    public class AttributeDefinition : ScriptableObject
    {
        [SerializeField] protected string _attributeName;
        public string AttributeName => _attributeName;
        [SerializeField] protected float _baseValue;
        [SerializeField, HideInInspector] private string _id;
        public string AttributeType => _id;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
                _id = System.Guid.NewGuid().ToString();
        }
        public virtual Attribute CreateRuntimeAttribute()
        {
            return new Attribute(_baseValue, _attributeName != "" ? _attributeName : "-No Name-");
        }
    }
}