using UnityEngine;
using AbilitySystem.Projectiles;

namespace AbilitySystem.Core
{
    [CreateAssetMenu(fileName = "SpawnProjectileAction", menuName = "Ability System/Ability Actions/SpawnProjectileAction")]
    public class SpawnProjectileActionDefinition : AbilityActionDefinition
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private SignalDefinition _projectileHitSignal;
        [SerializeField] private SignalDefinition _projectileDestroySignal;

        public override IAbilityAction CreateRuntimeAction()
        {
            return new SpawnProjectileAction(projectilePrefab, _projectileHitSignal, _projectileDestroySignal);
        }
    }
}