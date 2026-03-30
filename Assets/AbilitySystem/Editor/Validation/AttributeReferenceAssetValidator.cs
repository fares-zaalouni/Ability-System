using System.Collections.Generic;
using System.Linq;
using AbilitySystem.Attributes;
using AbilitySystem.Utility;
using UnityEditor;
using UnityEngine;

namespace AbilitySystem.Editor.Validation
{
    public static class AttributeReferenceAssetValidator
    {
        [MenuItem("Ability System/Validation/Validate Attribute References")]
        public static void ValidateAttributeReferences()
        {
            var attributeDefinitions = LoadAllAssetsOfType<AttributeDefinition>();
            var modifierDefinitions = LoadAllAssetsOfType<AttributeModifierDefinition>();

            var knownNames = attributeDefinitions
                .Select(d => d.AttributeName)
                .Where(AttributeNameValidator.IsValid)
                .Distinct(System.StringComparer.Ordinal)
                .ToList();

            var errorCount = 0;

            foreach (var modifier in modifierDefinitions)
            {
                var referencedName = modifier.AttributeName;

                if (!AttributeNameValidator.IsValid(referencedName))
                {
                    errorCount++;
                    Debug.LogError($"[Attribute Validation] Modifier '{AssetDatabase.GetAssetPath(modifier)}' has empty attribute name.", modifier);
                    continue;
                }

                if (!knownNames.Contains(referencedName, System.StringComparer.Ordinal))
                {
                    errorCount++;
                    var suggestions = AttributeNameValidator.SimilarAttributeNames(referencedName, knownNames);
                    var suggestionSuffix = string.IsNullOrEmpty(suggestions) ? string.Empty : $" Did you mean: {suggestions}?";

                    Debug.LogError(
                        $"[Attribute Validation] Modifier '{modifier.name}' references unknown attribute '{referencedName}'.{suggestionSuffix}",
                        modifier);
                }
            }

            if (errorCount == 0)
            {
                Debug.Log($"[Attribute Validation] Success. Checked {attributeDefinitions.Count} attribute definitions and {modifierDefinitions.Count} modifier definitions. No issues found.");
            }
            else
            {
                Debug.LogWarning($"[Attribute Validation] Completed with {errorCount} issue(s). See console errors for details.");
            }
        }

        private static List<T> LoadAllAssetsOfType<T>() where T : ScriptableObject
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(a => a != null)
                .ToList();
        }
    }
}