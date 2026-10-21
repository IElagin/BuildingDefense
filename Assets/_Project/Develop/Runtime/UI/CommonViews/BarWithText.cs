using Assets._Project.Develop.Runtime.UI.Core;
using TMPro;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.CommonViews
{
    public class BarWithText : MonoBehaviour, IView
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Bar _bar;

        // Game addition (L8 hordes): enemy bars show up only once the enemy is hurt, so a horde is not a wall of bars.
        [SerializeField] private bool _hiddenWhenFull;

        public void UpdateText(string text) => _text.text = text;

        public void UpdateSlider(float sliderValue)
        {
            _bar.UpdateValue(sliderValue);

            if (_hiddenWhenFull)
                gameObject.SetActive(sliderValue < 1f);
        }

        public void SetFillerColor(Color color) => _bar.SetFillerColor(color);
    }
}
