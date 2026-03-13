using System;

namespace AbilitySystem.Core
{
    public class Cooldown
    {
        public float Duration { get; private set; }
        public float RemainingTime { get; private set; }
        public event Action OnCooldownStarted;
        public event Action OnCooldownEnded;
        public bool IsOnCooldown => RemainingTime > 0f;
        public Cooldown(float duration)
        {
            Duration = duration;
            RemainingTime = 0f;
        }
        public void Tick(float deltaTime)
        {
            if (RemainingTime > 0f)
            {
                RemainingTime -= deltaTime;
                if (RemainingTime <= 0f)
                {
                    RemainingTime = 0f;
                    OnCooldownEnded?.Invoke();
                }
            }
        }
        public void ForceCooldownIfLonger(float duration)
        {
            if (duration > RemainingTime)
            {
                ForceCooldown(duration);
            }
        }
        public void ForceCooldown(float duration)
        {
            RemainingTime = duration;
            OnCooldownStarted?.Invoke();
        }
        public void Start()
        {
            if(RemainingTime > 0f)
            {
                return;
            }
            RemainingTime = Duration;
            OnCooldownStarted?.Invoke();
        }
    
    }
}