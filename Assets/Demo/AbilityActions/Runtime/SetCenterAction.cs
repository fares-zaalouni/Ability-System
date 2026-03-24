using AbilitySystem.Core;
using UnityEngine;

public class SetCenterAction : IAbilityAction
{
    private Vector3 _center;

    public SetCenterAction(Vector3 center)
    {
        _center = center;
    }

    public void Execute(AbilityContext context, AbilityRunner runner)
    {
        context.Set(new TargetPoint(_center));
        runner.Next();
    }
}