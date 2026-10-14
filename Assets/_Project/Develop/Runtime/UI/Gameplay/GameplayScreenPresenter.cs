using Assets._Project.Develop.Runtime.Configs.Gameplay.Defense;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.BuildingFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.Rest;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Wallet;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay
{
    // HUD: building health, current phase and wave, gold, rest countdown with the start-wave button.
    public class GameplayScreenPresenter : IPresenter
    {
        private readonly GameplayScreenView _screen;
        private readonly ProjectPresentersFactory _projectPresentersFactory;
        private readonly BuildingHolderService _buildingHolderService;
        private readonly StageProviderService _stageProviderService;
        private readonly RestPhaseService _restPhaseService;
        private readonly DefenseConfig _defenseConfig;

        private readonly List<IPresenter> _childPresenters = new();
        private readonly List<IDisposable> _subscriptions = new();

        private Entity _building;

        public GameplayScreenPresenter(
            GameplayScreenView screen,
            ProjectPresentersFactory projectPresentersFactory,
            BuildingHolderService buildingHolderService,
            StageProviderService stageProviderService,
            RestPhaseService restPhaseService,
            DefenseConfig defenseConfig)
        {
            _screen = screen;
            _projectPresentersFactory = projectPresentersFactory;
            _buildingHolderService = buildingHolderService;
            _stageProviderService = stageProviderService;
            _restPhaseService = restPhaseService;
            _defenseConfig = defenseConfig;
        }

        public void Initialize()
        {
            _screen.StartWaveButtonClicked += OnStartWaveButtonClicked;

            _childPresenters.Add(_projectPresentersFactory.CreateWalletPresenter(_screen.WalletView));

            foreach (IPresenter presenter in _childPresenters)
                presenter.Initialize();

            _building = _buildingHolderService.Building;

            UpdateHealth();
            UpdatePhase();
            UpdateRestTime(_restPhaseService.RemainingTime.Value);

            _subscriptions.Add(_building.CurrentHealth.Subscribe((oldValue, newValue) => UpdateHealth()));
            _subscriptions.Add(_stageProviderService.CurrentStageNumber.Subscribe((oldValue, newValue) => UpdatePhase()));
            _subscriptions.Add(_restPhaseService.IsActive.Subscribe((oldValue, newValue) => UpdatePhase()));
            _subscriptions.Add(_restPhaseService.RemainingTime.Subscribe((oldValue, newValue) => UpdateRestTime(newValue)));
        }

        public void Dispose()
        {
            _screen.StartWaveButtonClicked -= OnStartWaveButtonClicked;

            foreach (IDisposable subscription in _subscriptions)
                subscription.Dispose();

            _subscriptions.Clear();

            foreach (IPresenter presenter in _childPresenters)
                presenter.Dispose();

            _childPresenters.Clear();
        }

        private void OnStartWaveButtonClicked() => _restPhaseService.RequestSkip();

        private void UpdateHealth()
        {
            float health = _building.CurrentHealth.Value;
            float maxHealth = _building.MaxHealth.Value;
            int shownHealth = Mathf.CeilToInt(health);
            int shownMaxHealth = Mathf.CeilToInt(maxHealth);

            _screen.SetHealth(health / maxHealth, $"{shownHealth} / {shownMaxHealth}");
        }

        private void UpdatePhase()
        {
            bool isResting = _restPhaseService.IsActive.Value;
            int stagesCount = _stageProviderService.StagesCount;

            _screen.SetRestVisible(isResting);

            if (isResting)
            {
                int nextWaveNumber = _stageProviderService.CurrentStageNumber.Value + 1;

                _screen.SetPhase($"Отдых перед волной {nextWaveNumber}/{stagesCount}");
                _screen.SetHint($"Клик по земле — мина ({_defenseConfig.MinePrice} золота)");
                return;
            }

            int waveNumber = _stageProviderService.CurrentStageNumber.Value;

            _screen.SetPhase($"Волна {waveNumber}/{stagesCount}");
            _screen.SetHint("Клик по земле — взрыв");
        }

        private void UpdateRestTime(float remainingTime)
        {
            int seconds = Mathf.CeilToInt(remainingTime);

            _screen.SetRestTime($"{seconds} с");
        }
    }
}
