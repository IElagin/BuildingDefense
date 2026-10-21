using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewBomberConfig", fileName = "BomberConfig")]
    public class BomberConfig : EntityConfig
    {
        [field: SerializeField] public string PrefabPath { get; private set; } = "Entities/Bomber";
        [field: SerializeField, Min(0)] public float MoveSpeed { get; private set; } = 2.5f;
        [field: SerializeField, Min(0)] public float RotationSpeed { get; private set; } = 720;
        [field: SerializeField, Min(0)] public float MaxHealth { get; private set; } = 50;
        [field: SerializeField, Min(0)] public float DeathProcessTime { get; private set; } = 2.2f;
        [field: SerializeField, Min(0)] public float SpawnProcessTime { get; private set; } = 0.6f;
        [field: SerializeField, Min(0)] public float ReachDistance { get; private set; } = 2.2f;
        [field: SerializeField, Min(0)] public float BlastRadius { get; private set; } = 1.5f;
        [field: SerializeField, Min(0)] public float BlastDamage { get; private set; } = 10;
        [field: SerializeField] public ExplosionConfig BlastExplosion { get; private set; }
    }
}
