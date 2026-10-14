using Assets._Project.Develop.Runtime.Configs.Gameplay.Stages;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Levels
{
    [CreateAssetMenu(menuName = "Configs/Gameplay/Levels/NewLevelConfig", fileName = "LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] private List<StageConfig> _stageConfigs;

        [field: SerializeField, Min(1)] public float BuildingMaxHealth { get; private set; } = 150;
        [field: SerializeField, Min(0)] public float RestDuration { get; private set; } = 12;
        [field: SerializeField, Min(0)] public int WinGoldReward { get; private set; } = 50;

        public IReadOnlyList<StageConfig> StageConfigs => _stageConfigs;
    }
}
