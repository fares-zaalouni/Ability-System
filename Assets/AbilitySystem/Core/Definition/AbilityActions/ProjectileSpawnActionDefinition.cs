using UnityEngine;

[CreateAssetMenu(fileName = "SpawnProjectileAction", menuName = "Ability System/Ability Actions/SpawnProjectileAction")]


public class ProjectileSpawnActionDefinition : AbilityActionDefinition
{
    [SerializeField] Projectile projectilePrefab;
    public override IAbilityAction CreateRuntimeAction()
    {
        return new SpawnProjectileAction(projectilePrefab);
    }
}