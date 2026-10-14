using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using Cysharp.Threading.Tasks;

namespace Assets._Project.Develop.Runtime.UI.EndGamePopup
{
    public class EndGamePopupPresenter : PopupPresenterBase
    {
        private readonly EndGamePopupView _view;
        private readonly SceneSwitcherService _sceneSwitcherService;
        private readonly string _title;
        private readonly string _message;

        public EndGamePopupPresenter(
            EndGamePopupView view,
            SceneSwitcherService sceneSwitcherService,
            string title,
            string message)
        {
            _view = view;
            _sceneSwitcherService = sceneSwitcherService;
            _title = title;
            _message = message;
        }

        protected override PopupViewBase PopupView => _view;

        public override void Initialize()
        {
            base.Initialize();

            _view.SetTitle(_title);
            _view.SetMessage(_message);
            _view.MenuButtonClicked += OnMenuButtonClicked;
        }

        public override void Dispose()
        {
            base.Dispose();

            _view.MenuButtonClicked -= OnMenuButtonClicked;
        }

        private void OnMenuButtonClicked()
        {
            _sceneSwitcherService.ProcessSwitchTo(Scenes.MainMenu).Forget(AsyncErrors.Report);
        }
    }
}
