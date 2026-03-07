

using UnityEngine;
[CreateAssetMenu(fileName = "SetCenterAction", menuName = "Ability System/Ability Actions/SetCenterAction")]
public class SetCenterActionDefinition : AbilityActionDefinition
{
    [SerializeField] private Vector3 _center;

    public override IAbilityAction CreateRuntimeAction()
    {
        return new SetCenterAction(_center);
    }
}