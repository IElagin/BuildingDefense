using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.Rest;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Defense
{
    // A puddle is placed during rest, so the next rest starts when the wave after it is over.
    public class WaveOverDetectorSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly RestPhaseService _restPhaseService;

        private ReactiveVariable<bool> _isWaveOver;
        private IDisposable _restActiveDisposable;

        public WaveOverDetectorSystem(RestPhaseService restPhaseService)
        {
            _restPhaseService = restPhaseService;
        }

        public void OnInit(Entity entity)
        {
            _isWaveOver = entity.IsWaveOver;
            _restActiveDisposable = _restPhaseService.IsActive.Subscribe(OnRestActiveChanged);
        }

        public void OnDispose()
        {
            _restActiveDisposable.Dispose();
        }

        private void OnRestActiveChanged(bool oldValue, bool isActive)
        {
            if (isActive)
                _isWaveOver.Value = true;
        }
    }
}
