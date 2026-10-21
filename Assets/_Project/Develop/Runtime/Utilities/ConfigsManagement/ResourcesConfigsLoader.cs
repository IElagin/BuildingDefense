using Assets._Project.Develop.Runtime.Configs.Audio;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Defense;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagement;
using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities.ConfigsManagement
{
    public class ResourcesConfigsLoader : IConfigsLoader
    {
        private readonly ResourcesAssetsLoader _resources;

        private readonly Dictionary<Type, string> _configsResourcesPaths = new()
        {
            {typeof(StartWalletConfig), "Configs/Meta/Wallet/StartWalletConfig"},
            {typeof(CurrencyIconsConfig), "Configs/Meta/Wallet/CurrencyIconsConfig"},
            {typeof(LevelsListConfig), "Configs/Gameplay/Levels/LevelsListConfig"},
            {typeof(BuildingConfig), "Configs/Gameplay/Entities/BuildingConfig"},
            {typeof(DefenseConfig), "Configs/Gameplay/Defense/DefenseConfig"},
            {typeof(AudioConfig), "Configs/Audio/AudioConfig"},
        };

        public ResourcesConfigsLoader(ResourcesAssetsLoader resources)
        {
            _resources = resources;
        }

        public async UniTask<Dictionary<Type, object>> LoadAsync(CancellationToken cancellationToken = default)
        {
            Dictionary<Type, object> loadedConfigs = new();

            foreach (KeyValuePair<Type, string> configResourcesPath in _configsResourcesPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ScriptableObject config = _resources.Load<ScriptableObject>(configResourcesPath.Value);
                loadedConfigs.Add(configResourcesPath.Key, config);
                await UniTask.NextFrame(cancellationToken: cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            return loadedConfigs;
        }
    }
}
