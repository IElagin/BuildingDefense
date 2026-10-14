using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    // One explosion look and lifetime for every source: clicks, mines and bombers.
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewExplosionConfig", fileName = "ExplosionConfig")]
    public class ExplosionConfig : EntityConfig
    {
        [field: SerializeField] public string PrefabPath { get; private set; } = "Entities/Explosion";
        [field: SerializeField, Min(0.01f)] public float EffectTime { get; private set; } = 0.6f;
    }
}
