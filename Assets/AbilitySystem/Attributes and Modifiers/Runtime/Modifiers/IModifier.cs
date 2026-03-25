namespace AbilitySystem.Attributes
{
    public interface IModifier
    {
        public int Priority { get; }
        void Apply(Attribute attribute);
    }
}