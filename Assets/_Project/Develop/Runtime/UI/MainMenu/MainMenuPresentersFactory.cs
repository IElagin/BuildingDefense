using Assets._Project.Develop.Runtime.Meta.Features.Statistics;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagement;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using VContainer;

namespace Assets._Project.Develop.Runtime.UI.MainMenu
{
    public class MainMenuPresentersFactory
    {
        private readonly IObjectResolver _container;

        public MainMenuPresentersFactory(IObjectResolver container)
        {
            _container = container;
        }

        public MainMenuScreenPresenter CreateMainMenuScreen(MainMenuScreenView view)
        {
            return new MainMenuScreenPresenter(
                view,
                _container.Resolve<ProjectPresentersFactory>(),
                _container.Resolve<SceneSwitcherService>(),
                _container.Resolve<MainMenuPopupService>(),
                _container.Resolve<ConfigsProviderService>(),
                _container.Resolve<GameStatisticsService>(),
                _container.Resolve<IAudioService>());
        }
    }
}
