using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay.ResultsPopups;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayPopupService : PopupService
    {
        private readonly GameplayUIRoot _uiRoot;
        private readonly GameplayPresentersFactory _gameplayPresentersFactory;

        public GameplayPopupService(
            ViewsFactory viewsFactory,
            ProjectPresentersFactory presentersFactory,
            GameplayUIRoot uiRoot,
            GameplayPresentersFactory gameplayPresentersFactory)
            : base(viewsFactory, presentersFactory)
        {
            _uiRoot = uiRoot;
            _gameplayPresentersFactory = gameplayPresentersFactory;
        }

        protected override Transform PopupLayer => _uiRoot.PopupsLayer;

        // Course OpenWinPopup (L8); the game passes the level's gold reward to show it.
        public WinPopupPresenter OpenWinPopup(int goldReward, Action closedCallback = null)
        {
            WinPopupView view = ViewsFactory.Create<WinPopupView>(ViewIDs.WinPopup, PopupLayer);

            WinPopupPresenter popup = _gameplayPresentersFactory.CreateWinPopupPresenter(view, goldReward);

            OnPopupCreated(popup, view, closedCallback);

            return popup;
        }

        public DefeatPopupPresenter OpenDefeatPopup(Action closedCallback = null)
        {
            DefeatPopupView view = ViewsFactory.Create<DefeatPopupView>(ViewIDs.DefeatPopup, PopupLayer);

            DefeatPopupPresenter popup = _gameplayPresentersFactory.CreateDefeatPopupPresenter(view);

            OnPopupCreated(popup, view, closedCallback);

            return popup;
        }
    }
}
