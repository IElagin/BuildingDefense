using Assets._Project.Develop.Runtime.Configs.Meta.Abilities;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.Utilities.ConfigsManagement;
using Assets._Project.Develop.Runtime.Utilities.DataManagement;
using Assets._Project.Develop.Runtime.Utilities.DataManagement.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets._Project.Develop.Runtime.Meta.Features.AbilitiesUpgrade
{
    // Course StatsUpgradeService (L11): ability levels saved in PlayerData. Level 0 — not bought, the ability is off.
    public class AbilitiesUpgradeService : IDataReader<PlayerData>, IDataWriter<PlayerData>
    {
        private ConfigsProviderService _configsProviderService;

        private Dictionary<AbilityTypes, ReactiveVariable<int>> _abilityLevels = new();

        public AbilitiesUpgradeService(PlayerDataProvider playerDataProvider, ConfigsProviderService configsProviderService)
        {
            _configsProviderService = configsProviderService;

            playerDataProvider.RegisterReader(this);
            playerDataProvider.RegisterWriter(this);
        }

        public List<AbilityTypes> AvailableAbilities => _abilityLevels.Keys.ToList();

        public IReadOnlyVariable<int> GetLevelFor(AbilityTypes type)
            => _abilityLevels[type];

        private AbilitiesUpgradeConfig AbilitiesUpgradeConfig => _configsProviderService.GetConfig<AbilitiesUpgradeConfig>();

        public int GetMaxLevelFor(AbilityTypes type)
            => AbilitiesUpgradeConfig.GetAbilityConfig(type).Values.Count;

        public float GetValueFor(AbilityTypes type, int level)
            => level == 0 ? 0 : AbilitiesUpgradeConfig.GetAbilityConfig(type).Values[level - 1];

        public int GetTargetsCountFor(AbilityTypes type, int level)
        {
            List<int> targetsCounts = AbilitiesUpgradeConfig.GetAbilityConfig(type).TargetsCounts;

            return level == 0 || targetsCounts.Count < level ? 0 : targetsCounts[level - 1];
        }

        public float GetCurrentValueFor(AbilityTypes type)
            => GetValueFor(type, _abilityLevels[type].Value);

        public int GetCurrentTargetsCountFor(AbilityTypes type)
            => GetTargetsCountFor(type, _abilityLevels[type].Value);

        public CurrencyTypes GetUpgradeCostTypeFor(AbilityTypes type)
        {
            return AbilitiesUpgradeConfig.GetAbilityConfig(type).CostType;
        }

        public bool TryGetUpgradeCostFor(AbilityTypes type, out CurrencyTypes costType, out int cost)
        {
            AbilityUpgradeConfig abilityData = AbilitiesUpgradeConfig.GetAbilityConfig(type);

            if (abilityData.UpgradeCosts.Count <= _abilityLevels[type].Value)
            {
                costType = default(CurrencyTypes);
                cost = 0;
                return false;
            }

            costType = abilityData.CostType;
            cost = abilityData.UpgradeCosts[_abilityLevels[type].Value];
            return true;
        }

        public bool TryUpgradeAbility(AbilityTypes type)
        {
            if (GetMaxLevelFor(type) <= _abilityLevels[type].Value)
                return false;

            _abilityLevels[type].Value += 1;
            return true;
        }

        public void ReadFrom(PlayerData data)
        {
            // A save made before the shop has no AbilityLevels, a save made before a new ability has no key for it:
            // such abilities start not bought, as in GetOriginData.
            if (data.AbilityLevels != null)
            {
                foreach (var abilityLevel in data.AbilityLevels)
                    SetLevel(abilityLevel.Key, abilityLevel.Value);
            }

            foreach (AbilityTypes abilityType in Enum.GetValues(typeof(AbilityTypes)))
                if (data.AbilityLevels == null || data.AbilityLevels.ContainsKey(abilityType) == false)
                    SetLevel(abilityType, 0);
        }

        public void WriteTo(PlayerData data)
        {
            data.AbilityLevels ??= new();

            foreach (var ability in _abilityLevels)
                data.AbilityLevels[ability.Key] = ability.Value.Value;
        }

        private void SetLevel(AbilityTypes abilityType, int level)
        {
            if (_abilityLevels.ContainsKey(abilityType))
                _abilityLevels[abilityType].Value = level;
            else
                _abilityLevels.Add(abilityType, new ReactiveVariable<int>(level));
        }
    }
}
