using Assets._Project.Develop.Runtime.Meta.Features.AbilitiesUpgrade;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Utilities.DataManagement
{
    public class PlayerData : ISaveData
    {
        public Dictionary<CurrencyTypes, int> WalletData;
        public int Wins;
        public int Losses;
        public Dictionary<AbilityTypes, int> AbilityLevels;
    }
}
