
using UnityEngine;

public abstract class StatusEffect
{
    public AbilityEffectDefinition Definition { get; private set; }
    protected ICaster source;
    public float TickInterval { get; private set; }
    public float RemainingDuration { get; private set; }
    public float TimeUntilNextTick { get; private set; }
    protected int stacks;


    public StatusEffect(AbilityEffectDefinition definition, float duration, float tickInterval, int stacks, ICaster source)
    {
        this.RemainingDuration = duration;
        this.TickInterval = tickInterval;
        this.stacks = stacks;
        this.source = source;
        this.TimeUntilNextTick = tickInterval;
    }
    public void Tick(float deltaTime, IAbilityTarget target)
    {
        RemainingDuration -= deltaTime;
        TimeUntilNextTick -= deltaTime;

        // Check if DoT/Buff should trigger this frame
        if (TimeUntilNextTick <= 0f)
        {
            Debug.Log($"Ticking effect {Definition} on {target}. Remaining duration: {RemainingDuration}");
            ApplyTick(target);
            TimeUntilNextTick += TickInterval; // Reset tick timer
        }
    }

    public abstract void ApplyTick(IAbilityTarget target);
    

    public bool IsExpired => RemainingDuration <= 0f;

}