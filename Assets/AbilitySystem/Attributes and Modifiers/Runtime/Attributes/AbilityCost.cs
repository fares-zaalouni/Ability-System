namespace AbilitySystem.Attributes
{
    [System.Serializable]
    public struct AbilityCost
    {
        public string costId;
        private Attribute cost;

        public AbilityCost(string costId, float amount)
        {
            this.costId = costId;
            cost = new Attribute(amount);
        }
        public float Cost => cost.RuntimeValue;

        public void AddCostModifier(IModifier modifier)
        {
            cost.AddModifier(modifier);
        }
        public void RemoveCostModifier(IModifier modifier)
        {
            cost.RemoveModifier(modifier);
        }
    }
}