using Assets._Project.Develop.Runtime.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.AbilitiesShopPopup
{
    // Course UpgradableStatView (L11) plus the level text.
    public class UpgradableAbilityView : MonoBehaviour, IView
    {
        [SerializeField] private TMP_Text _abilityName;
        [SerializeField] private Image _abilityIcon;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _effectText;

        [field: SerializeField] public BuyButtonView BuyButtonView { get; private set; }

        public void Initialize(string abilityName, Sprite abilitySprite)
        {
            _abilityName.text = abilityName;
            _abilityIcon.sprite = abilitySprite;
        }

        public void SetLevelText(string level) => _levelText.text = level;

        public void SetEffectText(string effect) => _effectText.text = effect;
    }
}
