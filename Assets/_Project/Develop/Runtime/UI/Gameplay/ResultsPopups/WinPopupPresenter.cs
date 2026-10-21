using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using Cysharp.Threading.Tasks;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.ResultsPopups
{
    // Course WinPopupPresenter (L8); the game shows the gold reward of the level.
    public class WinPopupPresenter : PopupPresenterBase
    {
        private const string TitleName = "ПОБЕДА!";

        private readonly WinPopupView _view;
        private readonly SceneSwitcherService _sceneSwitcher;
        private readonly int _goldReward;

        public WinPopupPresenter(
            WinPopupView view,
            SceneSwitcherService sceneSwitcher,
            int goldReward)
        {
            _view = view;
            _sceneSwitcher = sceneSwitcher;
            _goldReward = goldReward;
        }

        protected override PopupViewBase PopupView => _view;

        public override void Initialize()
        {
            base.Initialize();

            _view.SetTitle(TitleName);
            _view.SetReward($"Купол выстоял. Награда: +{_goldReward}");

            _view.ContinueClicked += OnContinueClicked;
        }

        protected override void OnPreHide()
        {
            base.OnPreHide();

            _view.ContinueClicked -= OnContinueClicked;
        }

        public override void Dispose()
        {
            base.Dispose();

            _view.ContinueClicked -= OnContinueClicked;
        }

        private void OnContinueClicked()
        {
            _sceneSwitcher.ProcessSwitchTo(Scenes.MainMenu).Forget(AsyncErrors.Report);
            OnCloseRequest();
        }
    }
}
