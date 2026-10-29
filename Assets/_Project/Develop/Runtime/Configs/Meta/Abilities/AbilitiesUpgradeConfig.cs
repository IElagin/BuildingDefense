using Assets._Project.Develop.Runtime.Meta.Features.AbilitiesUpgrade;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Meta.Abilities
{
    // Course PlayerStatsUpgradeConfig (L11). Level 0 is not bought, so the lists start at level 1:
    // Values[i] and UpgradeCosts[i] belong to level i + 1.
    [CreateAssetMenu(menuName = "Configs/Meta/Abilities/NewAbilitiesUpgradeConfig", fileName = "AbilitiesUpgradeConfig")]
    public class AbilitiesUpgradeConfig : ScriptableObject
    {
        [SerializeField] private List<AbilityUpgradeConfig> _abilities = new List<AbilityUpgradeConfig>();

        public AbilityUpgradeConfig GetAbilityConfig(AbilityTypes type)
            => _abilities.First(a => a.Type == type);
    }

    [Serializable]
    public class AbilityUpgradeConfig
    {
        [field: SerializeField] public AbilityTypes Type { get; private set; }

        // percent: of the dome's max HP, of an enemy's max HP, or of the click damage
        [field: SerializeField] public List<float> Values { get; private set; }

        // how many enemies of a wave the ability hits; only the orbital volley uses it
        [field: SerializeField] public List<int> TargetsCounts { get; private set; }

        [field: SerializeField] public CurrencyTypes CostType { get; private set; } = CurrencyTypes.Diamond;
        [field: SerializeField] public List<int> UpgradeCosts { get; private set; }
    }
}
