using UnityEngine;
using AbilitySystem.Projectiles;

namespace AbilitySystem.Core
{
    public class SpawnProjectileAction : IAbilityAction
    {
        private Projectile _projectilePrefab;
        private Projectile _activeProjectile;
        private SignalDefinition _projectileHitSignal;
        private SignalDefinition _projectileDestroySignal;

        public SpawnProjectileAction(Projectile projectilePrefab, SignalDefinition projectileHitSignal = null, SignalDefinition projectileDestroySignal = null)
        {
            _projectilePrefab = projectilePrefab;
            _projectileHitSignal = projectileHitSignal;
            _projectileDestroySignal = projectileDestroySignal;
        }

        public void Execute(AbilityContext context, AbilityRunner runner)
        {
            if (!context.TryGet(ContextKeys.ProjectileSpawnPoint, out Vector3 spawnPoint))
            {
                Debug.LogError("Failed to spawn projectile: No spawn point found.");
                runner.Next();
                return;
            }
            _activeProjectile = Object.Instantiate(_projectilePrefab, spawnPoint, Quaternion.identity);

            // Create per-cast RuntimeSignal instances and publish them to context so that
            // consumer actions in this same pipeline can find them by slot key.
            RuntimeSignal hitSignal = null;
            RuntimeSignal destroySignal = null;

            if (_projectileHitSignal != null)
            {
                hitSignal = new RuntimeSignal();
                context.Set(_projectileHitSignal.ContextKey, hitSignal);
            }

            if (_projectileDestroySignal != null)
            {
                destroySignal = new RuntimeSignal();
                context.Set(_projectileDestroySignal.ContextKey, destroySignal);
            }

            _activeProjectile.OnHit += hitData =>
            {
                context.Set(ContextKeys.ProjectileHitData, hitData);
                hitSignal?.Raise(context);
            };
            _activeProjectile.OnDestroyed += destroyData =>
            {
                context.Set(ContextKeys.ProjectileDestroyData, destroyData);
                destroySignal?.Raise(context);
            };
            if(context.TryGet(ContextKeys.ProjectileLaunchDirection, out Vector3 launchDirection))
            {
                _activeProjectile.Launch(launchDirection);
            }
            runner.Next();
        }


    }
}