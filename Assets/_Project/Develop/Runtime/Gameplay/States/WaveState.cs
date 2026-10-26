using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.Defense;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StagesFeature;
using Assets._Project.Develop.Runtime.Utilities.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.States
{
    // A wave (the course stage process state): the next stage runs and the click puts an explosion request.
    public class WaveState : State, IUpdatableState
    {
        private readonly StageProviderService _stageProviderService;
        private readonly IInputService _inputService;
        private readonly GroundPointService _groundPointService;
        private readonly Entity _clickExplosionCaster;

        public WaveState(
            StageProviderService stageProviderService,
            IInputService inputService,
            GroundPointService groundPointService,
            Entity clickExplosionCaster)
        {
            _stageProviderService = stageProviderService;
            _inputService = inputService;
            _groundPointService = groundPointService;
            _clickExplosionCaster = clickExplosionCaster;
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

            if (_inputService.IsPointerClicked == false)
                return;

            if (_groundPointService.TryGetPoint(_inputService.PointerPosition, out Vector3 point))
                _clickExplosionCaster.ClickExplosionRequest.Invoke(point);
        }

        public override void Exit()
        {
            base.Exit();

            _stageProviderService.CleanupCurrent();
        }
    }
}
