using Assets._Project.Develop.Runtime.Gameplay.Features.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.Rest;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    // Rest before a wave (the course preparation state): the click places mines until the rest is over.
    public class RestState : State, IUpdatableState
    {
        private readonly RestPhaseService _restPhaseService;
        private readonly MinePlacementService _minePlacementService;

        public RestState(
            RestPhaseService restPhaseService,
            MinePlacementService minePlacementService)
        {
            _restPhaseService = restPhaseService;
            _minePlacementService = minePlacementService;
        }

        public override void Enter()
        {
            base.Enter();

            _restPhaseService.Start();
        }

        public void Update(float deltaTime)
        {
            _restPhaseService.Update();
            _minePlacementService.Update();
        }

        public override void Exit()
        {
            base.Exit();

            _restPhaseService.Stop();
        }
    }
}
