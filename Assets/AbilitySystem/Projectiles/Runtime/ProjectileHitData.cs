using UnityEngine;

namespace AbilitySystem.Projectiles
{
    public readonly struct ProjectileHitData
    {
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly GameObject HitObject;

        public ProjectileHitData(Vector3 point, Vector3 normal, GameObject hitObject = null)
        {
            Point = point;
            Normal = normal;
            HitObject = hitObject;
        }
    }
}