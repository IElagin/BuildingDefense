using System;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Statistics;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.MainMenu
{
    public class MainMenuScreenView : MonoBehaviour, IView
    {
        public event Action PlayButtonClicked;
        public event Action OpenShopButtonClicked;

        [field: SerializeField] public IconTextListView WalletView { get; private set; }
        [field: SerializeField] public StatisticsView StatisticsView { get; private set; }

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _openShopButton;

        private void OnEnable()
        {
            _playButton.onClick.AddListener(OnPlayButtonClicked);
            _openShopButton.onClick.AddListener(OnOpenShopButtonClicked);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(OnPlayButtonClicked);
            _openShopButton.onClick.RemoveListener(OnOpenShopButtonClicked);
        }

        private void OnPlayButtonClicked() => PlayButtonClicked?.Invoke();
        private void OnOpenShopButtonClicked() => OpenShopButtonClicked?.Invoke();
    }
}
