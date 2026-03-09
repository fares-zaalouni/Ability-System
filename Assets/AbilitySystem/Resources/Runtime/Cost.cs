namespace AbilitySystem.Resources
{
    [System.Serializable]
    public struct Cost
    {
        public string resourceName;
        public float amount;
        public Cost(string resourceName, float amount)
        {
            this.resourceName = resourceName;
            this.amount = amount;
        }
    }
}