using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Gameplay.Stages
{
    // A wave: every listed burrow releases its own enemies; the burrow positions live in the level config.
    [CreateAssetMenu(menuName = "Configs/Gameplay/Stages/NewWaveStage", fileName = "WaveStage")]
    public class WaveStageConfig : StageConfig
    {
        [SerializeField] private List<BurrowSpawnConfig> _burrowSpawns;

        public IReadOnlyList<BurrowSpawnConfig> BurrowSpawns => _burrowSpawns;
    }
}
