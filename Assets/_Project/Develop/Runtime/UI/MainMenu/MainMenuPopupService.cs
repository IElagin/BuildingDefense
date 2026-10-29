using Assets._Project.Develop.Runtime.UI.AbilitiesShopPopup;
using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.MainMenu
{
    // Course MainMenuPopupService (L11); the shop opens only from the menu, so its Open method lives here.
    public class MainMenuPopupService : PopupService
    {
        private readonly MainMenuUIRoot _uiRoot;

        public MainMenuPopupService(
            ViewsFactory viewsFactory,
            ProjectPresentersFactory presentersFactory,
            MainMenuUIRoot uiRoot)
            : base(viewsFactory, presentersFactory)
        {
            _uiRoot = uiRoot;
        }

        protected override Transform PopupLayer => _uiRoot.PopupsLayer;

        public AbilitiesShopPopupPresenter OpenAbilitiesShopPopup()
        {
            AbilitiesShopPopupView view = ViewsFactory.Create<AbilitiesShopPopupView>(ViewIDs.AbilitiesShopPopup, PopupLayer);

            AbilitiesShopPopupPresenter popup = PresentersFactory.CreateAbilitiesShopPopupPresenter(view);

            OnPopupCreated(popup, view);

            return popup;
        }
    }
}
