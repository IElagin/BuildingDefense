using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Defense
{
    // Player actions: the click explosion during a wave and buying mines during rest.
    [CreateAssetMenu(menuName = "Configs/Gameplay/Defense/NewDefenseConfig", fileName = "DefenseConfig")]
    public class DefenseConfig : ScriptableObject
    {
        [field: SerializeField] public ExplosionConfig ClickExplosion { get; private set; }
        [field: SerializeField, Min(0)] public float ClickExplosionRadius { get; private set; } = 2.5f;
        [field: SerializeField, Min(0)] public float ClickExplosionDamage { get; private set; } = 50;
        [field: SerializeField, Min(0)] public float ClickExplosionCooldown { get; private set; } = 0.6f;
        [field: SerializeField] public MineConfig Mine { get; private set; }
        [field: SerializeField, Min(0)] public int MinePrice { get; private set; } = 25;
        [field: SerializeField, Min(0)] public float MinMineDistanceToBuilding { get; private set; } = 3;
        [field: SerializeField, Min(0)] public float MaxMineDistanceToBuilding { get; private set; } = 11;
    }
}
