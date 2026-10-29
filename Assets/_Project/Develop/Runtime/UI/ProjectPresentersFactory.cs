using Assets._Project.Develop.Runtime.Configs.Meta.Abilities;
using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;
using Assets._Project.Develop.Runtime.Meta.Features.AbilitiesUpgrade;
using Assets._Project.Develop.Runtime.Meta.Features.Statistics;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.AbilitiesShopPopup;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Statistics;
using Assets._Project.Develop.Runtime.UI.Wallet;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagement;
using Assets._Project.Develop.Runtime.Utilities.DataManagement.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using VContainer;

namespace Assets._Project.Develop.Runtime.UI
{
    public class ProjectPresentersFactory
    {
        private readonly IObjectResolver _container;

        public ProjectPresentersFactory(IObjectResolver container)
        {
            _container = container;
        }

        public CurrencyPresenter CreateCurrencyPresenter(
            IconTextView view,
            IReadOnlyVariable<int> currency,
            CurrencyTypes currencyType)
        {
            return new CurrencyPresenter(
                currency,
                currencyType,
                _container.Resolve<ConfigsProviderService>().GetConfig<CurrencyIconsConfig>(),
                view);
        }

        public WalletPresenter CreateWalletPresenter(IconTextListView view)
        {
            return new WalletPresenter(
                _container.Resolve<WalletService>(),
                this,
                _container.Resolve<ViewsFactory>(),
                view);
        }

        public StatisticsPresenter CreateStatisticsPresenter(StatisticsView view)
        {
            return new StatisticsPresenter(
                _container.Resolve<GameStatisticsService>(),
                view);
        }

        public AbilitiesShopPopupPresenter CreateAbilitiesShopPopupPresenter(AbilitiesShopPopupView view)
        {
            return new AbilitiesShopPopupPresenter(
                view,
                this,
                _container.Resolve<AbilitiesUpgradeService>(),
                _container.Resolve<ViewsFactory>());
        }

        public UpgradableAbilityPresenter CreateUpgradableAbilityPresenter(UpgradableAbilityView view, AbilityTypes abilityType)
        {
            return new UpgradableAbilityPresenter(
                view,
                abilityType,
                _container.Resolve<ConfigsProviderService>().GetConfig<AbilitiesViewConfig>(),
                _container.Resolve<AbilitiesUpgradeService>(),
                _container.Resolve<WalletService>(),
                _container.Resolve<PlayerDataProvider>(),
                _container.Resolve<ConfigsProviderService>().GetConfig<CurrencyIconsConfig>(),
                _container.Resolve<IAudioService>());
        }
    }
}
