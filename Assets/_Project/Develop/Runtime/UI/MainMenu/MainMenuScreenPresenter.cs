using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Statistics;
using Assets._Project.Develop.Runtime.UI.Wallet;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagement;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.MainMenu
{
    public class MainMenuScreenPresenter : IPresenter
    {
        private readonly MainMenuScreenView _screen;

        private readonly ProjectPresentersFactory _projectPresentersFactory;

        private readonly SceneSwitcherService _sceneSwitcherService;

        private readonly MainMenuPopupService _popupService;

        private readonly ConfigsProviderService _configsProviderService;
        private readonly IAudioService _audio;

        private readonly List<IPresenter> _childPresenters = new();

        public MainMenuScreenPresenter(
            MainMenuScreenView screen,
            ProjectPresentersFactory projectPresentersFactory,
            SceneSwitcherService sceneSwitcherService,
            MainMenuPopupService popupService,
            ConfigsProviderService configsProviderService,
            IAudioService audio)
        {
            _screen = screen;
            _projectPresentersFactory = projectPresentersFactory;
            _sceneSwitcherService = sceneSwitcherService;
            _popupService = popupService;
            _configsProviderService = configsProviderService;
            _audio = audio;
        }

        public void Initialize()
        {
            _screen.PlayButtonClicked += OnPlayButtonClicked;
            _screen.OpenShopButtonClicked += OnOpenShopButtonClicked;

            CreateWallet();
            CreateStatistics();

            foreach (IPresenter presenter in _childPresenters)
                presenter.Initialize();
        }

        public void Dispose()
        {
            _screen.PlayButtonClicked -= OnPlayButtonClicked;
            _screen.OpenShopButtonClicked -= OnOpenShopButtonClicked;

            foreach (IPresenter presenter in _childPresenters)
                presenter.Dispose();

            _childPresenters.Clear();
        }

        private void CreateWallet()
        {
            WalletPresenter walletPresenter = _projectPresentersFactory.CreateWalletPresenter(_screen.WalletView);

            _childPresenters.Add(walletPresenter);
        }

        private void CreateStatistics()
        {
            StatisticsPresenter statisticsPresenter = _projectPresentersFactory.CreateStatisticsPresenter(_screen.StatisticsView);

            _childPresenters.Add(statisticsPresenter);
        }

        private void OnPlayButtonClicked()
        {
            _audio.Play(SoundIds.UIOpen);

            int levelsCount = _configsProviderService.GetConfig<LevelsListConfig>().Levels.Count;
            int levelNumber = Random.Range(1, levelsCount + 1);

            _sceneSwitcherService
                .ProcessSwitchTo(Scenes.Gameplay, new GameplayInputArgs(levelNumber)).Forget(AsyncErrors.Report);
        }

        private void OnOpenShopButtonClicked()
        {
            _audio.Play(SoundIds.UIOpen);
            _popupService.OpenAbilitiesShopPopup();
        }
    }
}
