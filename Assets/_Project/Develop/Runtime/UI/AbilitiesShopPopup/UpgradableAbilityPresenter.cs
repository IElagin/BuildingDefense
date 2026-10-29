using Assets._Project.Develop.Runtime.Configs.Meta.Abilities;
using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;
using Assets._Project.Develop.Runtime.Meta.Features.AbilitiesUpgrade;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.DataManagement.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.AbilitiesShopPopup
{
    // Course UpgradableStatPresenter (L11). The game saves right after a purchase (the course does not).
    public class UpgradableAbilityPresenter : IPresenter
    {
        private const string NextLevelColor = "#7CFF3A";

        private UpgradableAbilityView _view;
        private AbilitiesViewConfig _abilitiesViewConfig;
        private AbilitiesUpgradeService _abilitiesUpgradeService;
        private WalletService _walletService;
        private PlayerDataProvider _playerDataProvider;
        private AbilityTypes _abilityType;
        private CurrencyIconsConfig _currencyIconsConfig;
        private IAudioService _audio;

        private List<IDisposable> _disposables = new();

        public UpgradableAbilityPresenter(
            UpgradableAbilityView view,
            AbilityTypes type,
            AbilitiesViewConfig abilitiesViewConfig,
            AbilitiesUpgradeService abilitiesUpgradeService,
            WalletService walletService,
            PlayerDataProvider playerDataProvider,
            CurrencyIconsConfig currencyIconsConfig,
            IAudioService audio)
        {
            _view = view;
            _abilitiesViewConfig = abilitiesViewConfig;
            _abilitiesUpgradeService = abilitiesUpgradeService;
            _walletService = walletService;
            _playerDataProvider = playerDataProvider;
            _abilityType = type;
            _currencyIconsConfig = currencyIconsConfig;
            _audio = audio;
        }

        public UpgradableAbilityView View => _view;

        private AbilityViewConfig ViewData => _abilitiesViewConfig.GetAbilityViewData(_abilityType);

        public void Initialize()
        {
            _view.Initialize(ViewData.Name, ViewData.Sprite);
            UpdateLevelTexts();

            UpdateBuyButtonState();

            _view.BuyButtonView.Click += OnBuyButtonClicked;

            IReadOnlyVariable<int> abilityLevel = _abilitiesUpgradeService.GetLevelFor(_abilityType);
            _disposables.Add(abilityLevel.Subscribe(OnAbilityLevelChanged));

            IReadOnlyVariable<int> currency = _walletService.GetCurrency(_abilitiesUpgradeService.GetUpgradeCostTypeFor(_abilityType));
            _disposables.Add(currency.Subscribe(OnWalletChanged));
        }

        public void Dispose()
        {
            _view.BuyButtonView.Click -= OnBuyButtonClicked;

            foreach (IDisposable disposable in _disposables)
                disposable.Dispose();

            _disposables.Clear();
        }

        private void OnAbilityLevelChanged(int arg1, int arg2)
        {
            UpdateLevelTexts();
            UpdateBuyButtonState();
        }

        private void OnWalletChanged(int arg1, int arg2) => UpdateBuyButtonState();

        private void OnBuyButtonClicked()
        {
            if (_abilitiesUpgradeService.TryGetUpgradeCostFor(_abilityType, out CurrencyTypes currencyType, out int cost))
            {
                if (_walletService.Enough(currencyType, cost))
                {
                    if (_abilitiesUpgradeService.TryUpgradeAbility(_abilityType) == false)
                        throw new Exception();

                    _walletService.Spend(currencyType, cost);
                    _playerDataProvider.SaveAsync().Forget(AsyncErrors.Report);
                    _audio.Play(SoundIds.UIOpen);
                }
                else
                {
                    Debug.Log("Not enough currency");
                }
            }
            else
            {
                Debug.Log("Already max");
            }
        }

        private void UpdateBuyButtonState()
        {
            if (_abilitiesUpgradeService.TryGetUpgradeCostFor(_abilityType, out CurrencyTypes currencyType, out int cost))
            {
                _view.BuyButtonView.SetPriceText(cost.ToString());
                _view.BuyButtonView.ShowIcon();
                _view.BuyButtonView.SetIcon(_currencyIconsConfig.GetSpriteFor(currencyType));

                if (_walletService.Enough(currencyType, cost))
                    _view.BuyButtonView.Unlock();
                else
                    _view.BuyButtonView.Lock();
            }
            else
            {
                _view.BuyButtonView.HideIcon();
                _view.BuyButtonView.Lock();
                _view.BuyButtonView.SetPriceText("МАКС");
            }
        }

        private void UpdateLevelTexts()
        {
            int level = _abilitiesUpgradeService.GetLevelFor(_abilityType).Value;
            int maxLevel = _abilitiesUpgradeService.GetMaxLevelFor(_abilityType);

            _view.SetLevelText($"Ур. {level}/{maxLevel}");

            string result = level == 0 ? "Не куплено" : GetEffectText(level);

            if (level < maxLevel)
                result += $"\n<color={NextLevelColor}>» {GetEffectText(level + 1)}</color>";

            _view.SetEffectText(result);
        }

        private string GetEffectText(int level)
        {
            return string.Format(
                ViewData.EffectFormat,
                _abilitiesUpgradeService.GetValueFor(_abilityType, level),
                _abilitiesUpgradeService.GetTargetsCountFor(_abilityType, level));
        }
    }
}
