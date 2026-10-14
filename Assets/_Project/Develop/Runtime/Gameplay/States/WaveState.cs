using Assets._Project.Develop.Runtime.Gameplay.Features.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    // A wave (the course stage process state): the next stage runs and the click makes explosions.
    public class WaveState : State, IUpdatableState
    {
        private readonly StageProviderService _stageProviderService;
        private readonly ClickExplosionService _clickExplosionService;

        public WaveState(
            StageProviderService stageProviderService,
            ClickExplosionService clickExplosionService)
        {
            _stageProviderService = stageProviderService;
            _clickExplosionService = clickExplosionService;
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
            _clickExplosionService.Update();
        }

        public override void Exit()
        {
            base.Exit();

            _stageProviderService.CleanupCurrent();
        }
    }
}
