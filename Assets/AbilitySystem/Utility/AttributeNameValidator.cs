using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AbilitySystem.Utility
{
    public static class AttributeNameValidator
    {
        public static bool IsValid(string attributeName)
        {
            return !string.IsNullOrWhiteSpace(attributeName);
        }

        public static string SimilarAttributeNames(string attributeName, IEnumerable<string> candidateNames)
        {
            if (candidateNames == null)
            {
                return string.Empty;
            }

            var distinctCandidates = candidateNames
                .Where(IsValid)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var similarNames = new List<string>();
            foreach (var candidate in distinctCandidates)
            {
                if (IsSimilar(attributeName, candidate))
                {
                    similarNames.Add(candidate);
                }
            }

            return string.Join(", ", similarNames);
        }

        private static bool IsSimilar(string attributeName, string registeredName)
        {
            if (string.IsNullOrWhiteSpace(attributeName) || string.IsNullOrWhiteSpace(registeredName))
                return false;

            // Fast path: exact match ignoring case.
            if (string.Equals(attributeName, registeredName, StringComparison.OrdinalIgnoreCase))
                return true;

            // Normalization catches common format differences:
            // ability_power, ability-power, ability power, AbilityPower -> abilitypower
            var normalizedAttributeName = NormalizeForComparison(attributeName);
            var normalizedRegisteredName = NormalizeForComparison(registeredName);

            if (normalizedAttributeName.Length == 0 || normalizedRegisteredName.Length == 0)
                return false;

            if (string.Equals(normalizedAttributeName, normalizedRegisteredName, StringComparison.Ordinal))
                return true;

            // Adaptive threshold based on length so long names can tolerate small typos.
            var distance = LevenshteinDistance(normalizedAttributeName, normalizedRegisteredName);
            var maxLength = Math.Max(normalizedAttributeName.Length, normalizedRegisteredName.Length);
            var allowedDistance = maxLength <= 6 ? 1 : 2;

            return distance <= allowedDistance;
        }

        private static string NormalizeForComparison(string value)
        {
            var builder = new StringBuilder(value.Length);

            foreach (var c in value)
            {
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(char.ToLowerInvariant(c));
                }
            }

            return builder.ToString();
        }

        private static int LevenshteinDistance(string attributeName, string registeredName)
        {
            int[,] dp = new int[attributeName.Length + 1, registeredName.Length + 1];

            for (int i = 0; i <= attributeName.Length; i++)
                dp[i, 0] = i;
            for (int j = 0; j <= registeredName.Length; j++)
                dp[0, j] = j;

            for (int i = 1; i <= attributeName.Length; i++)
            {
                for (int j = 1; j <= registeredName.Length; j++)
                {
                    if (attributeName[i - 1] == registeredName[j - 1])
                        dp[i, j] = dp[i - 1, j - 1];
                    else
                        dp[i, j] = 1 + Math.Min(dp[i - 1, j - 1], Math.Min(dp[i - 1, j], dp[i, j - 1]));
                }
            }

            return dp[attributeName.Length, registeredName.Length];
        }
    }
}