using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    // A bug that stops at ReachDistance from the dome and spits acid: one spit per process + cooldown.
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewSpitterConfig", fileName = "SpitterConfig")]
    public class SpitterConfig : BugConfig
    {
        [field: SerializeField] public string PrefabPath { get; private set; } = "Entities/Spitter";
        [field: SerializeField, Min(0)] public float AttackProcessTime { get; private set; } = 0.5f;
        [field: SerializeField, Min(0)] public float AttackDelayTime { get; private set; } = 0.3f;
        [field: SerializeField, Min(0)] public float AttackCooldown { get; private set; } = 1.5f;
        [field: SerializeField] public string SpitPrefabPath { get; private set; } = "Entities/AcidSpit";
        [field: SerializeField, Min(0)] public float SpitSpeed { get; private set; } = 12;
        [field: SerializeField, Min(0)] public float SpitBlastRadius { get; private set; } = 1;
        [field: SerializeField, Min(0)] public float SpitBlastDamage { get; private set; } = 6;
        [field: SerializeField] public ExplosionConfig SpitExplosion { get; private set; }
    }
}
