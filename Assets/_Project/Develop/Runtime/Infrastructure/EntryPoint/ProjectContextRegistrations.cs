using VContainer;
using Assets._Project.Develop.Runtime.Meta.Features.Statistics;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagement;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagement;
using Assets._Project.Develop.Runtime.Utilities.DataManagement;
using Assets._Project.Develop.Runtime.Utilities.DataManagement.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.DataManagement.DataRepository;
using Assets._Project.Develop.Runtime.Utilities.DataManagement.KeysStorage;
using Assets._Project.Develop.Runtime.Utilities.DataManagement.Serializers;
using Assets._Project.Develop.Runtime.Utilities.LoadingScreen;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using Assets._Project.Develop.Runtime.Utilities.Timer;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.Infrastructure.EntryPoint
{
    public class ProjectContextRegistrations
    {
        public static void Process(IContainerBuilder container, CancellationToken projectToken)
        {
            StandardLoadingScreen loadingScreen = null;

            container.Register(CreateConfigsProviderService, Lifetime.Singleton);

            container.Register(CreateResourcesAssetsLoader, Lifetime.Singleton);

            container.Register(CreateSceneLoaderService, Lifetime.Singleton);

            container.Register(c => CreateSceneSwitcherService(c, projectToken), Lifetime.Singleton);

            container.Register<ILoadingScreen>(c => loadingScreen = CreateLoadingScreen(c), Lifetime.Singleton);

            container.Register(CreateWalletService, Lifetime.Singleton);

            container.Register(CreatePlayerDataProvider, Lifetime.Singleton);

            container.Register(CreateProjectPresentersFactory, Lifetime.Singleton);

            container.Register(CreateViewsFactory, Lifetime.Singleton);

            container.Register<ISaveLoadService>(CreateSaveLoadService, Lifetime.Singleton);

            container.Register(CreateTimerServiceFactory, Lifetime.Singleton);

            container.Register(CreateGameStatisticsService, Lifetime.Singleton);

            container.RegisterBuildCallback(c =>
            {
                c.Resolve<WalletService>();
                c.Resolve<GameStatisticsService>();
            });

            container.RegisterDisposeCallback(c =>
            {
                if (loadingScreen != null)
                    Object.Destroy(loadingScreen.gameObject);
            });
        }

        private static TimerServiceFactory CreateTimerServiceFactory(IObjectResolver c)
            => new TimerServiceFactory();

        private static GameStatisticsService CreateGameStatisticsService(IObjectResolver c)
            => new GameStatisticsService(c.Resolve<PlayerDataProvider>());

        private static ViewsFactory CreateViewsFactory(IObjectResolver c)
            => new ViewsFactory(c.Resolve<ResourcesAssetsLoader>());

        private static ProjectPresentersFactory CreateProjectPresentersFactory(IObjectResolver c)
            => new ProjectPresentersFactory(c);

        private static PlayerDataProvider CreatePlayerDataProvider(IObjectResolver c)
            => new PlayerDataProvider(c.Resolve<ISaveLoadService>(), c.Resolve<ConfigsProviderService>());

        private static SaveLoadService CreateSaveLoadService(IObjectResolver c)
        {
            IDataSerializer dataSerializer = new JsonSerializer();
            IDataKeysStorage dataKeysStorage = new MapDataKeysStorage();

            string saveFolderPath = Application.isEditor ? Application.dataPath : Application.persistentDataPath;

            IDataRepository dataRepository = new LocalFileDataRepository(saveFolderPath, "json");

            return new SaveLoadService(dataSerializer, dataKeysStorage, dataRepository);
        }

        private static WalletService CreateWalletService(IObjectResolver c)
        {
            Dictionary<CurrencyTypes, ReactiveVariable<int>> currencies = new();

            foreach (CurrencyTypes currencyType in Enum.GetValues(typeof(CurrencyTypes)))
                currencies[currencyType] = new ReactiveVariable<int>(0);

            return new WalletService(currencies, c.Resolve<PlayerDataProvider>());
        }

        private static SceneSwitcherService CreateSceneSwitcherService(IObjectResolver c, CancellationToken projectToken)
            => new SceneSwitcherService(
                c.Resolve<SceneLoaderService>(),
                c.Resolve<ILoadingScreen>(),
                c,
                projectToken);

        private static SceneLoaderService CreateSceneLoaderService(IObjectResolver c)
            => new SceneLoaderService();

        private static ConfigsProviderService CreateConfigsProviderService(IObjectResolver c)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = c.Resolve<ResourcesAssetsLoader>();

            ResourcesConfigsLoader resourcesConfigsLoader = new ResourcesConfigsLoader(resourcesAssetsLoader);

            return new ConfigsProviderService(resourcesConfigsLoader);
        }

        private static ResourcesAssetsLoader CreateResourcesAssetsLoader(IObjectResolver c)
            => new ResourcesAssetsLoader();

        private static StandardLoadingScreen CreateLoadingScreen(IObjectResolver c)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = c.Resolve<ResourcesAssetsLoader>();

            StandardLoadingScreen standardLoadingScreenPrefab = resourcesAssetsLoader
                .Load<StandardLoadingScreen>("Utilities/StandardLoadingScreen");

            return Object.Instantiate(standardLoadingScreenPrefab);
        }
    }
}
