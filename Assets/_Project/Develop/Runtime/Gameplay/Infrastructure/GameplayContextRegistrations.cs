using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Mono;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.Enemies;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.Rest;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.States;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Defense;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay;
using VContainer;
using Assets._Project.Develop.Runtime.Utilities.AssetsManagement;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagement;
using Assets._Project.Develop.Runtime.Utilities.Timer;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplayContextRegistrations
    {
        private static GameplayInputArgs _inputArgs;

        public static void Process(IContainerBuilder container, GameplayInputArgs args)
        {
            _inputArgs = args;

            EntitiesLifeContext entitiesLifeContext = null;
            MonoEntitiesFactory monoEntitiesFactory = null;

            container.Register(CreateEntitiesFactory, Lifetime.Scoped);

            container.Register(c => entitiesLifeContext ??= CreateEntitiesLifeContext(c), Lifetime.Transient);

            container.Register(CreateCollidersRegistryService, Lifetime.Scoped);

            container.Register(CreateBrainsFactory, Lifetime.Scoped);

            container.Register(CreateAIBrainsContext, Lifetime.Scoped);

            container.Register(CreateMainHeroFactory, Lifetime.Scoped);
            container.Register(CreateEnemiesFactory, Lifetime.Scoped);

            container.Register(CreateStagesFactory, Lifetime.Scoped);
            container.Register(CreateStageProviderService, Lifetime.Scoped);

            container.Register(CreateRestPhaseService, Lifetime.Scoped);

            container.Register(CreateGroundPointService, Lifetime.Scoped);
            container.Register(CreateExplosionAbilityService, Lifetime.Scoped);
            container.Register(CreateMinesPlacementService, Lifetime.Scoped);

            container.Register(CreateGameplayStatesFactory, Lifetime.Scoped);

            container.Register(CreateGameplayStatesContext, Lifetime.Scoped);

            container.Register(CreateMainHeroHolderService, Lifetime.Scoped);
            container.RegisterBuildCallback(c => c.Resolve<MainHeroHolderService>().Initialize());

            container.Register<IInputService>(CreateDesktopInput, Lifetime.Scoped);

            container.Register(CreateGameplayUIRoot, Lifetime.Scoped);
            container.Register(CreateGameplayPresentersFactory, Lifetime.Scoped);
            container.Register(CreateGameplayScreenPresenter, Lifetime.Scoped);
            container.Register(CreateGameplayPopupService, Lifetime.Scoped);

            container.Register(c => monoEntitiesFactory ??= CreateMonoEntitiesFactory(c), Lifetime.Transient);

            container.RegisterBuildCallback(c => c.Resolve<MonoEntitiesFactory>().Initialize());

            // VContainer disposes tracked services in reverse creation order; the course releases entities before views.
            container.RegisterDisposeCallback(c =>
            {
                entitiesLifeContext.Dispose();
                monoEntitiesFactory.Dispose();
            });
        }

        private static GameplayPopupService CreateGameplayPopupService(IObjectResolver c)
        {
            return new GameplayPopupService(
                c.Resolve<ViewsFactory>(),
                c.Resolve<ProjectPresentersFactory>(),
                c.Resolve<GameplayUIRoot>());
        }

        private static GameplayScreenPresenter CreateGameplayScreenPresenter(IObjectResolver c)
        {
            GameplayUIRoot uiRoot = c.Resolve<GameplayUIRoot>();

            GameplayScreenView view = c
                .Resolve<ViewsFactory>()
                .Create<GameplayScreenView>(ViewIDs.GameplayScreen, uiRoot.HUDLayer);

            return c
                .Resolve<GameplayPresentersFactory>()
                .CreateGameplayScreen(view);
        }

        private static GameplayPresentersFactory CreateGameplayPresentersFactory(IObjectResolver c)
        {
            return new GameplayPresentersFactory(c);
        }

        private static GameplayUIRoot CreateGameplayUIRoot(IObjectResolver c)
        {
            ResourcesAssetsLoader resourcesAssetsLoader = c.Resolve<ResourcesAssetsLoader>();

            GameplayUIRoot gameplayUIRootPrefab = resourcesAssetsLoader
                .Load<GameplayUIRoot>("UI/Gameplay/GameplayUIRoot");

            return Object.Instantiate(gameplayUIRootPrefab);
        }

        private static GameplayStatesContext CreateGameplayStatesContext(IObjectResolver c)
        {
            return new GameplayStatesContext(c.Resolve<GameplayStatesFactory>().CreateGameplayStateMachine(_inputArgs));
        }

        private static GameplayStatesFactory CreateGameplayStatesFactory(IObjectResolver c)
        {
            return new GameplayStatesFactory(c);
        }

        private static MainHeroHolderService CreateMainHeroHolderService(IObjectResolver c)
        {
            return new MainHeroHolderService(c.Resolve<EntitiesLifeContext>());
        }

        private static MinesPlacementService CreateMinesPlacementService(IObjectResolver c)
        {
            return new MinesPlacementService(
                c.Resolve<IInputService>(),
                c.Resolve<GroundPointService>(),
                c.Resolve<EntitiesFactory>(),
                c.Resolve<WalletService>(),
                c.Resolve<MainHeroHolderService>(),
                c.Resolve<ConfigsProviderService>().GetConfig<DefenseConfig>());
        }

        private static ExplosionAbilityService CreateExplosionAbilityService(IObjectResolver c)
        {
            return new ExplosionAbilityService(
                c.Resolve<IInputService>(),
                c.Resolve<GroundPointService>(),
                c.Resolve<EntitiesFactory>(),
                c.Resolve<ConfigsProviderService>().GetConfig<DefenseConfig>(),
                c.Resolve<TimerServiceFactory>());
        }

        private static GroundPointService CreateGroundPointService(IObjectResolver c)
        {
            return new GroundPointService();
        }

        private static RestPhaseService CreateRestPhaseService(IObjectResolver c)
        {
            return new RestPhaseService(
                c.Resolve<TimerServiceFactory>(),
                GetLevelConfig(c).RestDuration);
        }

        private static StageProviderService CreateStageProviderService(IObjectResolver c)
        {
            return new StageProviderService(
                GetLevelConfig(c),
                c.Resolve<StagesFactory>());
        }

        private static StagesFactory CreateStagesFactory(IObjectResolver c)
        {
            return new StagesFactory(c);
        }

        private static EnemiesFactory CreateEnemiesFactory(IObjectResolver c)
        {
            return new EnemiesFactory(c);
        }

        private static MainHeroFactory CreateMainHeroFactory(IObjectResolver c)
        {
            return new MainHeroFactory(c);
        }

        private static DesktopInput CreateDesktopInput(IObjectResolver c)
        {
            return new DesktopInput();
        }

        private static AIBrainsContext CreateAIBrainsContext(IObjectResolver c)
        {
            return new AIBrainsContext();
        }

        private static BrainsFactory CreateBrainsFactory(IObjectResolver c)
        {
            return new BrainsFactory(c);
        }

        private static CollidersRegistryService CreateCollidersRegistryService(IObjectResolver c)
        {
            return new CollidersRegistryService();
        }

        private static MonoEntitiesFactory CreateMonoEntitiesFactory(IObjectResolver c)
        {
            return new MonoEntitiesFactory(
                c.Resolve<ResourcesAssetsLoader>(),
                c.Resolve<EntitiesLifeContext>(),
                c.Resolve<CollidersRegistryService>());
        }

        private static EntitiesLifeContext CreateEntitiesLifeContext(IObjectResolver c)
        {
            return new EntitiesLifeContext();
        }

        private static EntitiesFactory CreateEntitiesFactory(IObjectResolver c)
        {
            return new EntitiesFactory(
                c.Resolve<EntitiesLifeContext>(),
                c.Resolve<MonoEntitiesFactory>(),
                c.Resolve<CollidersRegistryService>());
        }

        private static LevelConfig GetLevelConfig(IObjectResolver c)
            => c.Resolve<ConfigsProviderService>().GetConfig<LevelsListConfig>().GetBy(_inputArgs.LevelNumber);
    }
}
