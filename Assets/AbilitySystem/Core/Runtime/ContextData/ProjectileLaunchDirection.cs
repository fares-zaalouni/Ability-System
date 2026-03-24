using UnityEngine;

namespace AbilitySystem.Core
{
    public readonly struct ProjectileLaunchDirection
    {
        public Vector3 Value { get; }

        public ProjectileLaunchDirection(Vector3 value)
        {
            Value = value;
        }
    }
}
