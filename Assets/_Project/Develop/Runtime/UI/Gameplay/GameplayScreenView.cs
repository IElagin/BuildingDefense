using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayScreenView : MonoBehaviour, IView
    {
        public event Action StartWaveButtonClicked;

        [SerializeField] private Image _healthFill;
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private TMP_Text _phaseText;
        [SerializeField] private TMP_Text _hintText;
        [SerializeField] private GameObject _restPanel;
        [SerializeField] private TMP_Text _restTimeText;
        [SerializeField] private Button _startWaveButton;

        [field: SerializeField] public IconTextListView WalletView { get; private set; }

        public void SetHealth(float normalizedHealth, string text)
        {
            _healthFill.fillAmount = normalizedHealth;
            _healthText.text = text;
        }

        public void SetPhase(string text) => _phaseText.text = text;

        public void SetHint(string text) => _hintText.text = text;

        public void SetRestVisible(bool isVisible) => _restPanel.SetActive(isVisible);

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
