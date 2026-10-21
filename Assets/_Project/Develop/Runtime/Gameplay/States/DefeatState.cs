using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Statistics;
using Assets._Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Utilities.DataManagement.DataProviders;
using Assets._Project.Develop.Runtime.Utilities.SceneManagement;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class DefeatState : EndGameState, IUpdatableState
    {
        private readonly GameStatisticsService _statisticsService;
        private readonly PlayerDataProvider _playerDataProvider;
        private readonly GameplayPopupService _popupService;

        public DefeatState(
            IInputService inputService,
            GameStatisticsService statisticsService,
            PlayerDataProvider playerDataProvider,
            GameplayPopupService popupService) : base(inputService)
        {
            _statisticsService = statisticsService;
            _playerDataProvider = playerDataProvider;
            _popupService = popupService;
        }

        public override void Enter()
        {
            base.Enter();

            Debug.Log("ПОРАЖЕНИЕ!");

            _statisticsService.AddLoss();

            _playerDataProvider.SaveAsync().Forget(AsyncErrors.Report);

            _popupService.OpenDefeatPopup();
        }

        public void Update(float deltaTime)
        {
        }
    }
}
