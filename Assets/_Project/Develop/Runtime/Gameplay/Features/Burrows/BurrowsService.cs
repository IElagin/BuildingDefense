using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagement;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Burrows
{
    // Hive burrows of the level: crater props at the level config positions; waves release enemies from them.
    public class BurrowsService
    {
        private const string BurrowPrefabPath = "World/Burrow";
        private const float FullCircleDegrees = 360f;

        private readonly LevelConfig _levelConfig;
        private readonly ResourcesAssetsLoader _resourcesAssetsLoader;

        public BurrowsService(LevelConfig levelConfig, ResourcesAssetsLoader resourcesAssetsLoader)
        {
            _levelConfig = levelConfig;
            _resourcesAssetsLoader = resourcesAssetsLoader;
        }

        public void Create()
        {
            GameObject prefab = _resourcesAssetsLoader.Load<GameObject>(BurrowPrefabPath);

            foreach (Vector3 position in _levelConfig.BurrowPositions)
                Object.Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0f, FullCircleDegrees), 0));
        }

        public Vector3 GetPosition(int burrowIndex) => _levelConfig.BurrowPositions[burrowIndex];
    }
}
