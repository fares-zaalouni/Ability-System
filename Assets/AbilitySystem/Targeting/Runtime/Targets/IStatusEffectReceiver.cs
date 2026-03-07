public interface IStatusEffectReceiver
{
    void ApplyStatusEffectTo(StatusEffect statusEffect, ICaster source = null, int stacks = 1);
}