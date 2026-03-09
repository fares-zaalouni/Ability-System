using UnityEngine;
using AbilitySystem.Projectiles;

namespace AbilitySystem.Core
{
    public class SpawnProjectileAction : IAbilityAction
    {
        private Projectile _projectilePrefab;
        private Projectile _activeProjectile;
        
        public SpawnProjectileAction(Projectile projectilePrefab)
        {
            _projectilePrefab = projectilePrefab;
        }
        
        public void Execute(AbilityContext context, AbilityRunner runner)
        {
            context.TryGet(ContextKeys.ProjectileSpawnPoint, out Transform spawnPoint);
            if(spawnPoint == null)
            {
                Debug.LogError("SpawnProjectileAction requires a spawn point in the context.");
                return;
            }
            Projectile projectile = Object.Instantiate(_projectilePrefab, spawnPoint.position, spawnPoint.rotation);
            projectile.OnHit += (hitData) => {
                context.Set(ContextKeys.HitData, hitData);
                runner.Next();
            };
        }
    }
}