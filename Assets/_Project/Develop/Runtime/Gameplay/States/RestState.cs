using Assets._Project.Develop.Runtime.Gameplay.Features.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.Rest;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    // Rest before a wave (the course preparation state): the player builds defenses until the rest is over.
    public class RestState : State, IUpdatableState
    {
        private readonly RestPhaseService _restPhaseService;
        private readonly DefensePlacementService _defensePlacementService;

        public RestState(
            RestPhaseService restPhaseService,
            DefensePlacementService defensePlacementService)
        {
            _restPhaseService = restPhaseService;
            _defensePlacementService = defensePlacementService;
        }

        public override void Enter()
        {
            base.Enter();

            _restPhaseService.Start();
        }

        public void Update(float deltaTime)
        {
            _restPhaseService.Update();
            _defensePlacementService.Update();
        }

        public override void Exit()
        {
            base.Exit();

            _restPhaseService.Stop();
        }
    }
}
