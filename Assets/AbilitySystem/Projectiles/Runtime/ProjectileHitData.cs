using UnityEngine;

namespace AbilitySystem.Projectiles
{
    public readonly struct ProjectileHitData
    {
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly Collider HitObject;

        public ProjectileHitData(Vector3 point, Vector3 normal, Collider hitObject = null)
        {
            Point = point;
            Normal = normal;
            HitObject = hitObject;
        }
    }
}