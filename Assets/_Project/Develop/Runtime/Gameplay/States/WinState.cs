using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Statistics;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Utilities.DataManagement.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class WinState : EndGameState, IUpdatableState
    {
        private readonly WalletService _walletService;
        private readonly GameStatisticsService _statisticsService;
        private readonly PlayerDataProvider _playerDataProvider;
        private readonly GameplayPopupService _popupService;
        private readonly int _goldReward;

        public WinState(
            IInputService inputService,
            WalletService walletService,
            GameStatisticsService statisticsService,
            PlayerDataProvider playerDataProvider,
            GameplayPopupService popupService,
            int goldReward) : base(inputService)
        {
            _walletService = walletService;
            _statisticsService = statisticsService;
            _playerDataProvider = playerDataProvider;
            _popupService = popupService;
            _goldReward = goldReward;
        }

        public override void Enter()
        {
            base.Enter();

            Debug.Log("ПОБЕДА!");

            _walletService.Add(CurrencyTypes.Gold, _goldReward);
            _statisticsService.AddWin();

            _playerDataProvider.SaveAsync().Forget(AsyncErrors.Report);

            _popupService.OpenWinPopup(_goldReward);
        }

        public void Update(float deltaTime)
        {
        }
    }
}
