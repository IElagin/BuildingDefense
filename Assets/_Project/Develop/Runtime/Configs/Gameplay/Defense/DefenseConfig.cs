using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Defense
{
    // Player actions: the click explosion during a wave and buying defenses during rest.
    [CreateAssetMenu(menuName = "Configs/Gameplay/Defense/NewDefenseConfig", fileName = "DefenseConfig")]
    public class DefenseConfig : ScriptableObject
    {
        [field: SerializeField] public ExplosionConfig ClickExplosion { get; private set; }
        [field: SerializeField, Min(0)] public float ClickExplosionRadius { get; private set; } = 2.5f;
        [field: SerializeField, Min(0)] public float ClickExplosionDamage { get; private set; } = 50;
        [field: SerializeField, Min(0)] public float ClickExplosionCooldown { get; private set; } = 0.6f;
        [field: SerializeField] public MineConfig Mine { get; private set; }
        [field: SerializeField, Min(0)] public int MinePrice { get; private set; } = 25;
        [field: SerializeField] public TurretConfig Turret { get; private set; }
        [field: SerializeField] public PuddleConfig Puddle { get; private set; }
        [field: SerializeField, Min(0)] public int TurretPrice { get; private set; } = 60;
        [field: SerializeField, Min(0)] public int PuddlePrice { get; private set; } = 35;

        // the build ring around the building and the gap between two defenses
        [field: FormerlySerializedAs("<MinMineDistanceToBuilding>k__BackingField")]
        [field: SerializeField, Min(0)] public float MinDistanceToBuilding { get; private set; } = 3;
        [field: FormerlySerializedAs("<MaxMineDistanceToBuilding>k__BackingField")]
        [field: SerializeField, Min(0)] public float MaxDistanceToBuilding { get; private set; } = 13;
        [field: SerializeField, Min(0)] public float MinDistanceBetweenDefenses { get; private set; } = 1.5f;
    }
}
