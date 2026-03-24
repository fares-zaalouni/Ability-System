namespace AbilitySystem.Resources
{
    [System.Serializable]
    public struct AbilityCost
    {
        public string resourceName;
        private float originalCost;
        public float cost;

        public AbilityCost(string resourceName, float amount)
        {
            this.resourceName = resourceName;
            originalCost = amount;
            cost = amount;
        }
    }
}