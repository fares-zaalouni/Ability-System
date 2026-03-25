namespace AbilitySystem.Attributes
{
    [System.Serializable]
    public struct AbilityCost
    {
        public string attributeName;
        private float originalCost;
        public float cost;

        public AbilityCost(string resourceName, float amount)
        {
            this.attributeName = resourceName;
            originalCost = amount;
            cost = amount;
        }
    }
}