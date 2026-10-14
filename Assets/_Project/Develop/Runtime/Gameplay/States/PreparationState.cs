using Assets._Project.Develop.Runtime.Gameplay.Features.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.Rest;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    // Rest before a wave: the click places mines, Space or the HUD button starts the wave early.
    public class PreparationState : State, IUpdatableState
    {
        private readonly RestPhaseService _restPhaseService;
        private readonly MinesPlacementService _minesPlacementService;
        private readonly IInputService _inputService;

        public PreparationState(
            RestPhaseService restPhaseService,
            MinesPlacementService minesPlacementService,
            IInputService inputService)
        {
            _restPhaseService = restPhaseService;
            _minesPlacementService = minesPlacementService;
            _inputService = inputService;
        }

        public override void Enter()
        {
            base.Enter();

            _restPhaseService.Start();
        }

        public void Update(float deltaTime)
        {
            if (_inputService.IsStartWavePressed)
                _restPhaseService.RequestSkip();

            _minesPlacementService.Update();
        }

        public override void Exit()
        {
            base.Exit();

            _restPhaseService.Stop();
        }
    }
}
