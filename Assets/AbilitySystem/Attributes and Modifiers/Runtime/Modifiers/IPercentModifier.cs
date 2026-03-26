using Mono.Cecil;

namespace AbilitySystem.Attributes
{
    public interface IPercentModifier : IModifier
    {
        float Percent { get; }
    }
}