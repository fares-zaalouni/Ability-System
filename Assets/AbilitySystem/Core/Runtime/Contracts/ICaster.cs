namespace AbilitySystem.Core
{
    public interface ICaster
    {
        void GrantAbility(AbilityDefinition abilityDefinition);
        void RemoveAbility(AbilityDefinition abilityDefinition);
    }
}