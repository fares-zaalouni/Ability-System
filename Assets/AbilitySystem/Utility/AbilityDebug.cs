using System;
using UnityEngine;

namespace AbilitySystem.Utility
{
    public static class AbilityDebug
    {
        public static bool IsDevelopmentBuild => Debug.isDebugBuild || Application.isEditor;

        public static bool IsProductionBuild => !IsDevelopmentBuild;

        public static void Log(string message, UnityEngine.Object context = null)
        {
            if (!IsDevelopmentBuild)
                return;

            if (context != null)
                Debug.Log(message, context);
            else
                Debug.Log(message);
        }

        public static void LogWarning(string message, UnityEngine.Object context = null)
        {
            if (!IsDevelopmentBuild)
                return;

            if (context != null)
                Debug.LogWarning(message, context);
            else
                Debug.LogWarning(message);
        }

        public static void LogError(string message, UnityEngine.Object context = null)
        {
            if (!IsDevelopmentBuild)
                return;

            if (context != null)
                Debug.LogError(message, context);
            else
                Debug.LogError(message);
        }

        public static void LogException(Exception exception, UnityEngine.Object context = null)
        {
            if (!IsDevelopmentBuild)
                return;

            if (context != null)
                Debug.LogException(exception, context);
            else
                Debug.LogException(exception);
        }
    }
}
