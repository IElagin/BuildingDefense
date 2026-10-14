using Assets._Project.Develop.Runtime.Gameplay.Features.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    public class StageProcessState : State, IUpdatableState
    {
        private readonly StageProviderService _stageProviderService;
        private readonly ExplosionAbilityService _explosionAbilityService;

        public StageProcessState(
            StageProviderService stageProviderService,
            ExplosionAbilityService explosionAbilityService)
        {
            _stageProviderService = stageProviderService;
            _explosionAbilityService = explosionAbilityService;
        }

        public override void Enter()
        {
            base.Enter();

            _stageProviderService.SwitchToNext();
            _stageProviderService.StartCurrent();
        }

        public void Update(float deltaTime)
        {
            _stageProviderService.UpdateCurrent(deltaTime);
            _explosionAbilityService.Update();
        }

        public override void Exit()
        {
            base.Exit();

            _stageProviderService.CleanupCurrent();
        }
    }
}
