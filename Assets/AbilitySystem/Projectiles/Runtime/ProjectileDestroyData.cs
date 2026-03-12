using UnityEngine;

namespace AbilitySystem.Projectiles
{
    public readonly struct ProjectileDestroyData
    {
        public readonly Vector3 Point;
        public readonly Vector3 Normal;

        public ProjectileDestroyData(Vector3 point, Vector3 normal)
        {
            Point = point;
            Normal = normal;
        }
    }
}