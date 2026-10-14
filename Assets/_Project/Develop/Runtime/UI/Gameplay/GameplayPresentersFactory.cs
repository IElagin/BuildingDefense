using Assets._Project.Develop.Runtime.Configs.Gameplay.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.Features.Rest;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagement;
using VContainer;

namespace Assets._Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayPresentersFactory
    {
        private readonly IObjectResolver _container;

        public GameplayPresentersFactory(IObjectResolver container)
        {
            _container = container;
        }

        public GameplayScreenPresenter CreateGameplayScreen(GameplayScreenView view)
        {
            return new GameplayScreenPresenter(
                view,
                _container.Resolve<ProjectPresentersFactory>(),
                _container.Resolve<MainHeroHolderService>(),
                _container.Resolve<StageProviderService>(),
                _container.Resolve<RestPhaseService>(),
                _container.Resolve<ConfigsProviderService>().GetConfig<DefenseConfig>());
        }
    }
}
