using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.Rest;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay.Defense;
using Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay;
using Assets._Project.Develop.Runtime.UI.Gameplay.ResultsPopups;
using Assets._Project.Develop.Runtime.UI.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using VContainer;

namespace Assets._Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayPresentersFactory
    {
        private readonly IObjectResolver _container;
        private readonly GameplayInputArgs _gameplayInputArgs;

        public GameplayPresentersFactory(IObjectResolver container, GameplayInputArgs gameplayInputArgs)
        {
            _container = container;
            _gameplayInputArgs = gameplayInputArgs;
        }

        public WinPopupPresenter CreateWinPopupPresenter(WinPopupView view, int goldReward)
        {
            return new WinPopupPresenter(
                view,
                _container.Resolve<SceneSwitcherService>(),
                _container.Resolve<IAudioService>(),
                goldReward);
        }

        public DefeatPopupPresenter CreateDefeatPopupPresenter(DefeatPopupView view)
        {
            return new DefeatPopupPresenter(
                view,
                _container.Resolve<SceneSwitcherService>(),
                _container.Resolve<IAudioService>(),
                _gameplayInputArgs);
        }

        public GameplayScreenPresenter CreateGameplayScreen(GameplayScreenView view)
        {
            return new GameplayScreenPresenter(
                view,
                this,
                _container.Resolve<ProjectPresentersFactory>(),
                _container.Resolve<StageProviderService>(),
                _container.Resolve<RestPhaseService>(),
                _container.Resolve<DefensePlacementService>(),
                _container.Resolve<IAudioService>());
        }

        public StagePresenter CreateStagePresenter(IconTextView view)
        {
            return new StagePresenter(view, _container.Resolve<StageProviderService>());
        }

        public DefensePanelPresenter CreateDefensePanelPresenter(DefensePanelView view)
        {
            return new DefensePanelPresenter(
                view,
                _container.Resolve<DefensePlacementService>(),
                _container.Resolve<WalletService>(),
                _container.Resolve<IAudioService>());
        }

        public PlacementPreviewPresenter CreatePlacementPreviewPresenter()
        {
            ViewsFactory viewsFactory = _container.Resolve<ViewsFactory>();

            return new PlacementPreviewPresenter(
                viewsFactory.Create<PlacementPreviewView>(ViewIDs.PlacementPreview),
                viewsFactory,
                _container.Resolve<DefensePlacementService>(),
                _container.Resolve<RestPhaseService>());
        }

        public EntityHealthPresenter CreateEntityHealthPresenter(Entity entity, BarWithText view)
        {
            return new EntityHealthPresenter(entity, view);
        }

        public EntitiesHealthDisplayPresenter CreateEntitiesHealthDisplayPresenter(EntitiesHealthDisplay view)
        {
            return new EntitiesHealthDisplayPresenter(
                _container.Resolve<EntitiesLifeContext>(),
                view,
                _container.Resolve<ViewsFactory>(),
                this);
        }
    }
}
