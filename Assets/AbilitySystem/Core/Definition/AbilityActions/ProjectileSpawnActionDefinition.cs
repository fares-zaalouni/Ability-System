using UnityEngine;
using AbilitySystem.Projectiles;

namespace AbilitySystem.Core
{
    [CreateAssetMenu(fileName = "SpawnProjectileAction", menuName = "Ability System/Ability Actions/SpawnProjectileAction")]


    public class ProjectileSpawnActionDefinition : AbilityActionDefinition
    {
        [SerializeField] Projectile projectilePrefab;
        public override IAbilityAction CreateRuntimeAction()
        {
            return new SpawnProjectileAction(projectilePrefab);
        }
    }
}