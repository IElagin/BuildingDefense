using Assets._Project.Develop.Runtime.Meta.Features.AbilitiesUpgrade;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Meta.Abilities
{
    // Course StatsViewConfig (L11) plus the effect text: {0} — the level's value, {1} — its targets count.
    [CreateAssetMenu(menuName = "Configs/Meta/Abilities/NewAbilitiesViewConfig", fileName = "AbilitiesViewConfig")]
    public class AbilitiesViewConfig : ScriptableObject
    {
        [SerializeField] private List<AbilityViewConfig> _abilities;

        public AbilityViewConfig GetAbilityViewData(AbilityTypes type) => _abilities.First(a => a.Type == type);
    }

    [Serializable]
    public class AbilityViewConfig
    {
        [field: SerializeField] public AbilityTypes Type { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string EffectFormat { get; private set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }
    }
}
