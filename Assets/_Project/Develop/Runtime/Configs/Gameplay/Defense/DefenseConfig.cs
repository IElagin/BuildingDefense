using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Defense
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Defense/NewDefenseConfig", fileName = "DefenseConfig")]
    public class DefenseConfig : ScriptableObject
    {
        [field: SerializeField, Min(0)] public float ExplosionRadius { get; private set; } = 2.5f;
        [field: SerializeField, Min(0)] public float ExplosionDamage { get; private set; } = 50;
        [field: SerializeField, Min(0)] public float ExplosionCooldown { get; private set; } = 0.6f;
        [field: SerializeField, Min(0)] public int MinePrice { get; private set; } = 25;
        [field: SerializeField, Min(0)] public float MineTriggerRadius { get; private set; } = 1.2f;
        [field: SerializeField, Min(0)] public float MineBlastRadius { get; private set; } = 3;
        [field: SerializeField, Min(0)] public float MineBlastDamage { get; private set; } = 100;
        [field: SerializeField, Min(0)] public float MinMineDistanceToBuilding { get; private set; } = 3;
        [field: SerializeField, Min(0)] public float MaxMineDistanceToBuilding { get; private set; } = 11;
    }
}
