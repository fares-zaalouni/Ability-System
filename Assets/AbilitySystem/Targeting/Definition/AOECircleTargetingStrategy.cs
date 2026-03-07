using UnityEngine;

[CreateAssetMenu(fileName = "AOECircleTargetingStrategy", menuName = "Ability System/Targeting Strategies/AOECircleTargetingStrategy")]
public class AOECircleTargetingStrategy : TargetingStrategyDefinition
{
    [SerializeField] float radius;

    public override ITargetingStrategy CreateRuntimeStrategy()
    {
        return new AOECircleStrategy(radius);
    }
}
