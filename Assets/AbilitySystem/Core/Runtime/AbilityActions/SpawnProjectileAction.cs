using UnityEngine;
using AbilitySystem.Projectiles;
using AbilitySystem.Utility;

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
            if (!context.TryGet<ProjectileSpawnPoint>(out var projectileSpawnPoint))
            {
                AbilityDebug.LogError("Failed to spawn projectile: No spawn point found.");
                runner.Next();
                return;
            }
            Vector3 spawnPoint = projectileSpawnPoint.Value;
            _activeProjectile = Object.Instantiate(_projectilePrefab, spawnPoint, Quaternion.identity);

            // Create per-cast RuntimeSignal instances and publish them to context so that
            // consumer actions in this same pipeline can find them by slot key.
            RuntimeSignal hitSignal = null;
            RuntimeSignal destroySignal = null;

            if (_projectileHitSignal != null)
            {
                hitSignal = new RuntimeSignal();
                context.SetRuntimeSignal(_projectileHitSignal, hitSignal);
            }

            if (_projectileDestroySignal != null)
            {
                destroySignal = new RuntimeSignal();
                context.SetRuntimeSignal(_projectileDestroySignal, destroySignal);
            }

            _activeProjectile.OnHit += hitData =>
            {
                context.Set(hitData);
                hitSignal?.Raise(context);
            };
            _activeProjectile.OnDestroyed += destroyData =>
            {
                context.Set(destroyData);
                destroySignal?.Raise(context);
            };
            if (context.TryGet<ProjectileLaunchDirection>(out var projectileLaunchDirection))
            {
                _activeProjectile.Launch(projectileLaunchDirection.Value);
            }
            runner.Next();
        }


    }
}