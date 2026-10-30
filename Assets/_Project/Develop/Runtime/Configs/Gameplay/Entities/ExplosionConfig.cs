using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    // One explosion look and lifetime for all its sources.
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewExplosionConfig", fileName = "ExplosionConfig")]
    public class ExplosionConfig : EntityConfig
    {
        [field: SerializeField] public string PrefabPath { get; private set; } = "Entities/OrbitalStrike";
        [field: SerializeField, Min(0.01f)] public float EffectTime { get; private set; } = 0.6f;

        // Speed (m/s) given to the enemies this explosion kills; they fly off by physics. 0 — no flight.
        [field: SerializeField, Min(0)] public float ImpulseForce { get; private set; } = 9f;
    }
}
