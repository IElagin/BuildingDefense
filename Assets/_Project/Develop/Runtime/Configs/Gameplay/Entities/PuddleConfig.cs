using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Entities
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Entities/NewPuddleConfig", fileName = "PuddleConfig")]
    public class PuddleConfig : EntityConfig
    {
        [field: SerializeField] public string PrefabPath { get; private set; } = "Entities/Puddle";
        [field: SerializeField, Min(0)] public float Radius { get; private set; } = 2;
        [field: SerializeField, Min(0)] public float Damage { get; private set; } = 10;
        [field: SerializeField, Min(0.05f)] public float DamagePeriod { get; private set; } = 0.5f;
    }
}
