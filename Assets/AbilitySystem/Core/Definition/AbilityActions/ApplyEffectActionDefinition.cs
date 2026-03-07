using UnityEngine;

[CreateAssetMenu(fileName = "ApplyEffectAction", menuName = "Ability System/Ability Actions/ApplyEffectAction")]
public class ApplyEffectActionDefinition : AbilityActionDefinition
{
    [SerializeField] AbilityEffectDefinition abilityEffectDefinition;
    public override IAbilityAction CreateRuntimeAction()
    {
        return new ApplyEffectAction(abilityEffectDefinition);
    }
}
