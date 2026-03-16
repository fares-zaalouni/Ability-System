namespace AbilitySystem.Effects
{
    public class BasicStackingPolicyDefinition : StackingPolicyDefinition
    {
        public override IStackingPolicy CreateRuntimeStackingStrategy()
        {
            return new BasicStackingPolicy(_durationRefreshPolicy, _stackingBehavior, _stackIfSameSource, _newInstance);
        }
    }
}