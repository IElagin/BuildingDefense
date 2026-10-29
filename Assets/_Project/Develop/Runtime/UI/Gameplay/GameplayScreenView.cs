using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay.Defense;
using Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.Gameplay
{
    // Course GameplayScreenView (L8: stage number, entity health bars) plus the game's phase,
    // hint, rest countdown with the start-wave button, the defense cards (rest only) and the wallet.
    public class GameplayScreenView : MonoBehaviour, IView
    {
        public event Action StartWaveButtonClicked;

        [SerializeField] private TMP_Text _phaseText;
        [SerializeField] private TMP_Text _hintText;
        [SerializeField] private GameObject _restPanel;
        [SerializeField] private TMP_Text _restTimeText;
        [SerializeField] private Button _startWaveButton;

        [field: SerializeField] public IconTextView StageNumberView { get; private set; }

        [field: SerializeField] public EntitiesHealthDisplay EntitiesHealthDisplay { get; private set; }

        [field: SerializeField] public IconTextListView WalletView { get; private set; }

        [field: SerializeField] public DefensePanelView DefensePanel { get; private set; }

        public void SetPhase(string text) => _phaseText.text = text;

        public void SetHint(string text) => _hintText.text = text;

        public void SetRestVisible(bool isVisible)
        {
            _restPanel.SetActive(isVisible);
            DefensePanel.gameObject.SetActive(isVisible);
        }

        public void SetRestTime(string text) => _restTimeText.text = text;

        private void OnEnable()
        {
            _startWaveButton.onClick.AddListener(OnStartWaveButtonClicked);
        }

        private void OnDisable()
        {
            _startWaveButton.onClick.RemoveListener(OnStartWaveButtonClicked);
        }

        private void OnStartWaveButtonClicked() => StartWaveButtonClicked?.Invoke();
    }
}
