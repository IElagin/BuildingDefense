using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Stages
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Stages/NewWaveStage", fileName = "WaveStage")]
    public class WaveStageConfig : StageConfig
    {
        [field: SerializeField] public EntityConfig EnemyConfig { get; private set; }
        [field: SerializeField, Min(1)] public int EnemiesCount { get; private set; } = 4;
        [field: SerializeField, Min(0)] public float SpawnInterval { get; private set; } = 1.5f;
        [field: SerializeField, Min(0)] public float SpawnRadius { get; private set; } = 11.5f;
    }
}
