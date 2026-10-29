using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    // A shot comes once per AttackProcessTime + AttackCooldown; the bolt leaves after AttackDelayTime.
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewTurretConfig", fileName = "TurretConfig")]
    public class TurretConfig : EntityConfig
    {
        [field: SerializeField] public string PrefabPath { get; private set; } = "Entities/Turret";
        [field: SerializeField, Min(0)] public float AttackRadius { get; private set; } = 6;
        [field: SerializeField, Min(0)] public float Damage { get; private set; } = 20;
        [field: SerializeField, Min(0)] public float RotationSpeed { get; private set; } = 540;
        [field: SerializeField, Min(0)] public float AttackProcessTime { get; private set; } = 0.2f;
        [field: SerializeField, Min(0)] public float AttackDelayTime { get; private set; } = 0.05f;
        [field: SerializeField, Min(0)] public float AttackCooldown { get; private set; } = 0.6f;
    }
}
