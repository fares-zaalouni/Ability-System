using AbilitySystem.Core;


namespace AbilitySystem
{
    [System.Serializable]
    public struct LabeledAbility
    {
        public string Label;
        public AbilityDefinition Definition;

        public LabeledAbility(string label, AbilityDefinition definition)
        {
            Label = label;
            Definition = definition;
        }
    }
}
