using System.Collections.Generic;
using UnityEngine;

public class ConditionalActionDefinition : AbilityActionDefinition
{
    [SerializeField] private ConditionDefinition _condition;
    [SerializeField] private List<AbilityActionDefinition> _trueActions;
    [SerializeField] private List<AbilityActionDefinition> _falseActions;

    public override IAbilityAction CreateRuntimeAction()
    {
        return new ConditionalAction(
            _condition,
            _trueActions.ConvertAll(a => a.CreateRuntimeAction()),
            _falseActions.ConvertAll(a => a.CreateRuntimeAction())
        );
    }
}