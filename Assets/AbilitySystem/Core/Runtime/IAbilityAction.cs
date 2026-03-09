namespace AbilitySystem.Core
{
    public interface IAbilityAction
    {
        void Execute(AbilityContext context, AbilityRunner runner);
    }
}