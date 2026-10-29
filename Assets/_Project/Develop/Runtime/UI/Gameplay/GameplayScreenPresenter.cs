using Assets._Project.Develop.Runtime.Gameplay.Features.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.Rest;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay.Defense;
using Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay;
using Assets._Project.Develop.Runtime.UI.Gameplay.Stages;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay
{
    // Course GameplayScreenPresenter (L8: stage number, entity health bars with LateUpdate) plus the game HUD:
    // current phase and hint, gold, rest countdown with the start-wave button, the defense cards and the placement
    // ring. The building's health is its health bar above the dome. Plays the HUD sounds: wave start, denied place.
    public class GameplayScreenPresenter : IPresenter
    {
        private readonly GameplayScreenView _screen;
        private readonly GameplayPresentersFactory _gameplayPresentersFactory;
        private readonly ProjectPresentersFactory _projectPresentersFactory;
        private readonly StageProviderService _stageProviderService;
        private readonly RestPhaseService _restPhaseService;
        private readonly DefensePlacementService _defensePlacementService;
        private readonly IAudioService _audio;

        private readonly List<IPresenter> _childPresenters = new();
        private readonly List<IDisposable> _subscriptions = new();

        private EntitiesHealthDisplayPresenter _entitiesHealthDisplayPresenter;
        private PlacementPreviewPresenter _placementPreviewPresenter;

        public GameplayScreenPresenter(
            GameplayScreenView screen,
            GameplayPresentersFactory gameplayPresentersFactory,
            ProjectPresentersFactory projectPresentersFactory,
            StageProviderService stageProviderService,
            RestPhaseService restPhaseService,
            DefensePlacementService defensePlacementService,
            IAudioService audio)
        {
            _screen = screen;
            _gameplayPresentersFactory = gameplayPresentersFactory;
            _projectPresentersFactory = projectPresentersFactory;
            _stageProviderService = stageProviderService;
            _restPhaseService = restPhaseService;
            _defensePlacementService = defensePlacementService;
            _audio = audio;
        }

        public void Initialize()
        {
            _screen.StartWaveButtonClicked += OnStartWaveButtonClicked;

            CreateStageNumber();
            CreateEntitiesHealthDisplay();
            CreateDefense();
            _childPresenters.Add(_projectPresentersFactory.CreateWalletPresenter(_screen.WalletView));

            foreach (IPresenter presenter in _childPresenters)
                presenter.Initialize();

            UpdatePhase();
            UpdateRestTime(_restPhaseService.RemainingTime.Value);

            _subscriptions.Add(_stageProviderService.CurrentStageNumber.Subscribe(OnStageChanged));
            _subscriptions.Add(_restPhaseService.IsActive.Subscribe((oldValue, newValue) => UpdatePhase()));
            _subscriptions.Add(_restPhaseService.RemainingTime.Subscribe((oldValue, newValue) => UpdateRestTime(newValue)));
            _subscriptions.Add(_defensePlacementService.Denied.Subscribe(() => _audio.Play(SoundIds.MineDenied)));
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

        public void LateUpdate()
        {
            _entitiesHealthDisplayPresenter.LateUpdate();
            _placementPreviewPresenter.LateUpdate();
        }

        private void CreateStageNumber()
        {
            StagePresenter stagePresenter = _gameplayPresentersFactory.CreateStagePresenter(_screen.StageNumberView);

            _childPresenters.Add(stagePresenter);
        }

        private void CreateEntitiesHealthDisplay()
        {
            _entitiesHealthDisplayPresenter = _gameplayPresentersFactory
                .CreateEntitiesHealthDisplayPresenter(_screen.EntitiesHealthDisplay);

            _childPresenters.Add(_entitiesHealthDisplayPresenter);
        }

        private void CreateDefense()
        {
            _childPresenters.Add(_gameplayPresentersFactory.CreateDefensePanelPresenter(_screen.DefensePanel));

            _placementPreviewPresenter = _gameplayPresentersFactory.CreatePlacementPreviewPresenter();
            _childPresenters.Add(_placementPreviewPresenter);
        }

        private void OnStartWaveButtonClicked()
        {
            _audio.Play(SoundIds.UIClick);
            _restPhaseService.RequestSkip();
        }

        private void OnStageChanged(int oldNumber, int newNumber)
        {
            if (newNumber > oldNumber)
                _audio.Play(SoundIds.WaveStart);

            UpdatePhase();
        }

        private void UpdatePhase()
        {
            bool isResting = _restPhaseService.IsActive.Value;

            _screen.SetRestVisible(isResting);

            if (isResting)
            {
                int nextWaveNumber = _stageProviderService.CurrentStageNumber.Value + 1;

                _screen.SetPhase($"Подготовка к волне {nextWaveNumber}");
                _screen.SetHint("1–3 — постройка, клик по земле — поставить");
                return;
            }

            _screen.SetPhase($"Волна {_stageProviderService.CurrentStageNumber.Value}");
            _screen.SetHint("Клик по земле — орбитальный удар");
        }

        private void UpdateRestTime(float remainingTime)
        {
            int seconds = Mathf.CeilToInt(remainingTime);

            _screen.SetRestTime($"{seconds} с");
        }
    }
}
