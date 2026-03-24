using UnityEngine;

namespace AbilitySystem.Core
{
    public readonly struct TargetPoint
    {
        public Vector3 Value { get; }

        public TargetPoint(Vector3 value)
        {
            Value = value;
        }
    }
}
