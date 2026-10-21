using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Stages
{
    // How many enemies one burrow of the level releases during a wave (course EnemyItemConfig, with a burrow instead of a point).
    [Serializable]
    public class BurrowSpawnConfig
    {
        [field: SerializeField, Min(0)] public int BurrowIndex { get; private set; }
        [field: SerializeField] public EntityConfig EnemyConfig { get; private set; }
        [field: SerializeField, Min(0)] public int Count { get; private set; } = 10;
        [field: SerializeField, Min(0)] public float SpawnInterval { get; private set; } = 0.4f;
        [field: SerializeField, Min(0)] public float StartDelay { get; private set; }
    }
}
