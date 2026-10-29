using Assets._Project.Develop.Runtime.UI.Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.Defense
{
    // One defense card of the rest HUD: icon, price and key; highlighted when selected, dimmed without gold.
    public class DefenseCardView : MonoBehaviour, IView
    {
        public event Action Clicked;

        [SerializeField] private Button _button;
        [SerializeField] private Image _background;
        [SerializeField] private Sprite _normalSprite;
        [SerializeField] private Sprite _selectedSprite;
        [SerializeField] private CanvasGroup _content;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField, Range(0, 1)] private float _dimmedAlpha = 0.45f;

        public void SetPrice(string text) => _priceText.text = text;

        public void SetSelected(bool isSelected) => _background.sprite = isSelected ? _selectedSprite : _normalSprite;

        public void SetDimmed(bool isDimmed) => _content.alpha = isDimmed ? _dimmedAlpha : 1f;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked() => Clicked?.Invoke();
    }
}
