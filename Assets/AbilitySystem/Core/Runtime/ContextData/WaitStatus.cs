namespace AbilitySystem.Core
{
    public readonly struct WaitStatus
    {
        public bool Elapsed { get; }
        public float Duration { get; }
        public float MaxDuration { get; }

        public WaitStatus(bool elapsed, float duration, float maxDuration)
        {
            Elapsed = elapsed;
            Duration = duration;
            MaxDuration = maxDuration;
        }
    }
}
