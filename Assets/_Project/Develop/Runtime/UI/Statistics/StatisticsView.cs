using Assets._Project.Develop.Runtime.UI.Core;
using TMPro;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Statistics
{
    public class StatisticsView : MonoBehaviour, IView
    {
        [SerializeField] private TMP_Text _winsText;
        [SerializeField] private TMP_Text _lossesText;

        public void SetWins(string text) => _winsText.text = text;

        public void SetLosses(string text) => _lossesText.text = text;
    }
}
