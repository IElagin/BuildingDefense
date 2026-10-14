using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Gameplay.Features.BuildingFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.Rest;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Statistics;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Utilities.Conditions;
using Assets._Project.Develop.Runtime.Utilities.DataManagement.DataProviders;
using VContainer;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class GameplayStatesFactory
    {
        private readonly IObjectResolver _container;

        public GameplayStatesFactory(IObjectResolver container)
        {
            _container = container;
        }

        public RestState CreateRestState()
        {
            return new RestState(
                _container.Resolve<RestPhaseService>(),
                _container.Resolve<MinePlacementService>());
        }

        public WaveState CreateWaveState()
        {
            return new WaveState(
                _container.Resolve<StageProviderService>(),
                _container.Resolve<ClickExplosionService>());
        }

        public WinState CreateWinState()
        {
            return new WinState(
                _container.Resolve<IInputService>(),
                _container.Resolve<WalletService>(),
                _container.Resolve<GameStatisticsService>(),
                _container.Resolve<PlayerDataProvider>(),
                _container.Resolve<GameplayPopupService>(),
                _container.Resolve<LevelConfig>().WinGoldReward);
        }

        public DefeatState CreateDefeatState()
        {
            return new DefeatState(
                _container.Resolve<IInputService>(),
                _container.Resolve<GameStatisticsService>(),
                _container.Resolve<PlayerDataProvider>(),
                _container.Resolve<GameplayPopupService>());
        }

        public GameplayStateMachine CreateGameplayStateMachine()
        {
            StageProviderService stageProviderService = _container.Resolve<StageProviderService>();
            BuildingHolderService buildingHolderService = _container.Resolve<BuildingHolderService>();

            GameplayStateMachine coreLoopState = CreateCoreLoopState();

            DefeatState defeatState = CreateDefeatState();
            WinState winState = CreateWinState();

            ICompositeCondition coreLoopToWinStateCondition = new CompositeCondition()
                .Add(new FuncCondition(() => stageProviderService.CurrentStageResult.Value == StageResults.Completed))
                .Add(new FuncCondition(() => stageProviderService.HasNextStage() == false));

            ICompositeCondition coreLoopToDefeatStateCondition = new CompositeCondition()
                .Add(new FuncCondition(() =>
                {
                    if (buildingHolderService.Building != null)
                        return buildingHolderService.Building.IsDead.Value;

                    return false;
                }));

            GameplayStateMachine gameplayCycle = new GameplayStateMachine();

            gameplayCycle.AddState(coreLoopState);
            gameplayCycle.AddState(winState);
            gameplayCycle.AddState(defeatState);

            gameplayCycle.AddTransition(coreLoopState, winState, coreLoopToWinStateCondition);
            gameplayCycle.AddTransition(coreLoopState, defeatState, coreLoopToDefeatStateCondition);

            return gameplayCycle;
        }

        public GameplayStateMachine CreateCoreLoopState()
        {
            RestPhaseService restPhaseService = _container.Resolve<RestPhaseService>();
            StageProviderService stageProviderService = _container.Resolve<StageProviderService>();

            RestState restState = CreateRestState();
            WaveState waveState = CreateWaveState();

            ICompositeCondition restToWaveCondition = new CompositeCondition()
                .Add(new FuncCondition(() => restPhaseService.IsOver))
                .Add(new FuncCondition(() => stageProviderService.HasNextStage()));

            FuncCondition waveToRestCondition =
                new FuncCondition(() => stageProviderService.CurrentStageResult.Value == StageResults.Completed);

            GameplayStateMachine coreLoopState = new GameplayStateMachine();

            coreLoopState.AddState(restState);
            coreLoopState.AddState(waveState);

            coreLoopState.AddTransition(restState, waveState, restToWaveCondition);
            coreLoopState.AddTransition(waveState, restState, waveToRestCondition);

            return coreLoopState;
        }
    }
}
