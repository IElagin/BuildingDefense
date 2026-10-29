using Assets._Project.Develop.Runtime.UI.Core;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.Defense
{
    // The rest HUD row of defense cards, in the order of keys 1, 2, 3.
    public class DefensePanelView : MonoBehaviour, IView
    {
        [field: SerializeField] public DefenseCardView MineCard { get; private set; }
        [field: SerializeField] public DefenseCardView TurretCard { get; private set; }
        [field: SerializeField] public DefenseCardView PuddleCard { get; private set; }
    }
}
