namespace AbilitySystem.Core
{
    public static class ContextKeys
    {
        // Projectile-related context keys
        public const string ProjectileHitData = "ProjectileHitData";
        public const string ProjectileSpawnPoint = "ProjectileSpawnPoint";
        public const string ProjectileDestroyData = "ProjectileDestroyData";
        public const string ProjectileLaunchDirection = "ProjectileLaunchDirection";
        public const string ProjectileDirection = "ProjectileDirection";

        public const string TargetPoint = "TargetPoint";
        public const string AOECenter = "AOECenter";
        public const string WaitElapsed = "WaitElapsed";
        public const string WaitDuration = "WaitDuration";
        public const string MaxWaitDuration = "MaxWaitDuration";
        public const string RepeatTickedTimes = "RepeatTickedTimes";

        // Effect apply outcome summary keys
        public const string EffectApplyTotalTargets = "EffectApplyTotalTargets";
        public const string EffectApplyAppliedCount = "EffectApplyAppliedCount";
        public const string EffectApplySkippedCount = "EffectApplySkippedCount";
        public const string EffectApplyFailedCount = "EffectApplyFailedCount";
    }
}