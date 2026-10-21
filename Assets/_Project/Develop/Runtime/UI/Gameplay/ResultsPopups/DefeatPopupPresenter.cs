using Assets._Project.Develop.Runtime.Gameplay.Infrastructure;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using Cysharp.Threading.Tasks;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.ResultsPopups
{
    public class DefeatPopupPresenter : PopupPresenterBase
    {
        private const string TitleName = "ПОРАЖЕНИЕ";

        private readonly DefeatPopupView _view;
        private readonly SceneSwitcherService _sceneSwitcher;
        private readonly GameplayInputArgs _currentLevelArgs;

        public DefeatPopupPresenter(
            DefeatPopupView view,
            SceneSwitcherService sceneSwitcher,
            GameplayInputArgs currentLevelArgs)
        {
            _view = view;
            _sceneSwitcher = sceneSwitcher;
            _currentLevelArgs = currentLevelArgs;
        }

        protected override PopupViewBase PopupView => _view;

        public override void Initialize()
        {
            base.Initialize();

            _view.SetTitle(TitleName);

            _view.ContinueClicked += OnContinueClicked;
            _view.RestartClicked += OnRestartClicked;
        }

        protected override void OnPreHide()
        {
            base.OnPreHide();

            _view.ContinueClicked -= OnContinueClicked;
            _view.RestartClicked -= OnRestartClicked;
        }

        public override void Dispose()
        {
            base.Dispose();

            _view.ContinueClicked -= OnContinueClicked;
            _view.RestartClicked -= OnRestartClicked;
        }

        private void OnContinueClicked()
        {
            _sceneSwitcher.ProcessSwitchTo(Scenes.MainMenu).Forget(AsyncErrors.Report);
            OnCloseRequest();
        }

        private void OnRestartClicked()
        {
            _sceneSwitcher
                .ProcessSwitchTo(Scenes.Gameplay, new GameplayInputArgs(_currentLevelArgs.LevelNumber))
                .Forget(AsyncErrors.Report);
            OnCloseRequest();
        }
    }
}
