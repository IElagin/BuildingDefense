using Assets._Project.Develop.Runtime.Meta.Features.AbilitiesUpgrade;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Wallet;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.UI.AbilitiesShopPopup
{
    // Course StatsUpgradePopupPresenter (L11) without the character preview.
    public class AbilitiesShopPopupPresenter : PopupPresenterBase
    {
        private const string TitleName = "Магазин";

        private readonly AbilitiesShopPopupView _view;
        private readonly ViewsFactory _viewsFactory;
        private readonly ProjectPresentersFactory _projectPresentersFactory;
        private readonly AbilitiesUpgradeService _abilitiesUpgradeService;

        private List<UpgradableAbilityPresenter> _upgradableAbilityPresenters = new();
        private WalletPresenter _walletPresenter;

        public AbilitiesShopPopupPresenter(
            AbilitiesShopPopupView view,
            ProjectPresentersFactory projectPresentersFactory,
            AbilitiesUpgradeService abilitiesUpgradeService,
            ViewsFactory viewsFactory)
        {
            _view = view;
            _projectPresentersFactory = projectPresentersFactory;
            _abilitiesUpgradeService = abilitiesUpgradeService;
            _viewsFactory = viewsFactory;
        }

        protected override PopupViewBase PopupView => _view;

        public override void Initialize()
        {
            base.Initialize();

            _view.SetTitle(TitleName);

            _walletPresenter = _projectPresentersFactory.CreateWalletPresenter(_view.CurrencyListView);
            _walletPresenter.Initialize();

            foreach (AbilityTypes abilityType in _abilitiesUpgradeService.AvailableAbilities)
            {
                UpgradableAbilityView upgradableAbilityView = _viewsFactory.Create<UpgradableAbilityView>(ViewIDs.UpgradableAbilityView);
                _view.UpgradableAbilityListView.Add(upgradableAbilityView);

                UpgradableAbilityPresenter upgradableAbilityPresenter = _projectPresentersFactory
                    .CreateUpgradableAbilityPresenter(upgradableAbilityView, abilityType);
                _upgradableAbilityPresenters.Add(upgradableAbilityPresenter);
                upgradableAbilityPresenter.Initialize();
            }
        }

        protected override void OnPreHide()
        {
            base.OnPreHide();

            foreach (UpgradableAbilityPresenter presenter in _upgradableAbilityPresenters)
                presenter.Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (UpgradableAbilityPresenter presenter in _upgradableAbilityPresenters)
            {
                presenter.Dispose();
                _view.UpgradableAbilityListView.Remove(presenter.View);
                _viewsFactory.Release(presenter.View);
            }

            _upgradableAbilityPresenters.Clear();

            _walletPresenter.Dispose();
        }
    }
}
