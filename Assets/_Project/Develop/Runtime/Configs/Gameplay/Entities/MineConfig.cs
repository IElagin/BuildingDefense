using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewMineConfig", fileName = "MineConfig")]
    public class MineConfig : EntityConfig
    {
        [field: SerializeField] public string PrefabPath { get; private set; } = "Entities/Mine";
        [field: SerializeField, Min(0)] public float TriggerRadius { get; private set; } = 1.2f;
        [field: SerializeField, Min(0)] public float BlastRadius { get; private set; } = 3;
        [field: SerializeField, Min(0)] public float BlastDamage { get; private set; } = 100;
        [field: SerializeField] public ExplosionConfig BlastExplosion { get; private set; }
    }
}
