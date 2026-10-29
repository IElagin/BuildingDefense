using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewBomberConfig", fileName = "BomberConfig")]
    public class BomberConfig : BugConfig
    {
        [field: SerializeField] public string PrefabPath { get; private set; } = "Entities/Bomber";
        [field: SerializeField, Min(0)] public float BlastRadius { get; private set; } = 1.5f;
        [field: SerializeField, Min(0)] public float BlastDamage { get; private set; } = 10;
        [field: SerializeField] public ExplosionConfig BlastExplosion { get; private set; }
    }
}
