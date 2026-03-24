namespace AbilitySystem.Core
{
    public readonly struct EffectApplySummary
    {
        public int TotalTargets { get; }
        public int AppliedCount { get; }
        public int SkippedCount { get; }
        public int FailedCount { get; }

        public EffectApplySummary(int totalTargets, int appliedCount, int skippedCount, int failedCount)
        {
            TotalTargets = totalTargets;
            AppliedCount = appliedCount;
            SkippedCount = skippedCount;
            FailedCount = failedCount;
        }
    }
}
