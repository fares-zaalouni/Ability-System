using UnityEngine;

namespace AbilitySystem.Core
{
    public readonly struct ProjectileSpawnPoint
    {
        public Vector3 Value { get; }

        public ProjectileSpawnPoint(Vector3 value)
        {
            Value = value;
        }
    }
}
