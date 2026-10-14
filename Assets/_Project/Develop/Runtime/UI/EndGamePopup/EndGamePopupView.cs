using Assets._Project.Develop.Runtime.UI.Core;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.EndGamePopup
{
    public class EndGamePopupView : PopupViewBase
    {
        private const float TextFadeTime = 0.2f;

        public event Action MenuButtonClicked;

        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private Button _menuButton;

        public void SetTitle(string text) => _titleText.text = text;

        public void SetMessage(string text) => _messageText.text = text;

        protected override void ModifyShowAnimation(Sequence animation)
        {
            base.ModifyShowAnimation(animation);

            animation
                .Append(_messageText
                    .DOFade(1, TextFadeTime)
                    .From(0));
        }

        private void OnEnable()
        {
            _menuButton.onClick.AddListener(OnMenuButtonClicked);
        }

        private void OnDisable()
        {
            _menuButton.onClick.RemoveListener(OnMenuButtonClicked);
        }

        private void OnMenuButtonClicked() => MenuButtonClicked?.Invoke();
    }
}
