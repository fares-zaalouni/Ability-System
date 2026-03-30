using System.Collections.Generic;
using AbilitySystem.Attributes;
using AbilitySystem.Core;
using AbilitySystem.Targeting;
using AbilitySystem.Utility;

namespace AbilitySystem.Effects
{
    public static class ModifierResolutionHelper
    {
        public static void ResolveAndApplyModifiers(
            List<IModifier> modifiers,
            Attribute destinationAttribute,
            AbilityContext context,
            IAbilityTarget target,
            string effectLogPrefix)
        {
            if (modifiers == null || destinationAttribute == null || context == null)
                return;

            var targetAttributeHolder = target as IAttributeHolder;
            var sourceAttributeHolder = context.Caster as IAttributeHolder;

            foreach (var modifier in modifiers)
            {
                if (!(modifier is AttributeModifier attributeModifier))
                    continue;

                if (attributeModifier.Source == ModifierSource.Caster)
                {
                    AbilityDebug.Log($"Processing {effectLogPrefix} modifier from Caster. Attribute: {attributeModifier.AttributeName}, Percent: {attributeModifier.Percent:F4}");
                    if (sourceAttributeHolder == null)
                    {
                        AbilityDebug.LogError($"{effectLogPrefix} Modifier Error: Caster is not an attribute holder. Source: {context.Caster}");
                        continue;
                    }

                    if (sourceAttributeHolder.TryGetAttribute(attributeModifier.AttributeName, out var sourceAttr))
                    {
                        attributeModifier.SetBonusAttribute(sourceAttr);
                        destinationAttribute.AddModifier(attributeModifier);
                    }
                    else
                    {
                        AbilityDebug.LogError($"{effectLogPrefix} Modifier Error: Caster does not have the required attribute. Attribute Name: {attributeModifier.AttributeName}");
                    }
                }

                if (attributeModifier.Source == ModifierSource.Target)
                {
                    if (targetAttributeHolder == null)
                    {
                        AbilityDebug.LogError($"{effectLogPrefix} Modifier Error: Target is not an attribute holder. Target: {target}");
                        continue;
                    }

                    if (targetAttributeHolder.TryGetAttribute(attributeModifier.AttributeName, out var targetAttr))
                    {
                        attributeModifier.SetBonusAttribute(targetAttr);
                        destinationAttribute.AddModifier(attributeModifier);
                    }
                    else
                    {
                        AbilityDebug.LogError($"{effectLogPrefix} Modifier Error: Target does not have the required attribute. Attribute Name: {attributeModifier.AttributeName}");
                    }
                }
            }
        }
    }
}