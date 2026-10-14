using Assets._Project.Develop.Runtime.Utilities.Reactive;
using Assets._Project.Develop.Runtime.Utilities.Timer;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.Rest
{
    // Rest between waves: ends when its timer runs out or when the player starts the next wave earlier.
    public class RestPhaseService : IDisposable
    {
        private readonly TimerService _timer;
        private readonly ReactiveVariable<bool> _isActive = new();

        private bool _isSkipRequested;

        public RestPhaseService(TimerServiceFactory timerServiceFactory, float duration)
        {
            _timer = timerServiceFactory.Create(duration);
        }

        public IReadOnlyVariable<bool> IsActive => _isActive;
        public IReadOnlyVariable<float> RemainingTime => _timer.CurrentTime;
        public bool IsOver => _isSkipRequested || _timer.IsOver;

        public void Start()
        {
            _isSkipRequested = false;
            _timer.Restart();
            _isActive.Value = true;
        }

        public void RequestSkip()
        {
            if (_isActive.Value)
                _isSkipRequested = true;
        }

        public void Stop()
        {
            _timer.Stop();
            _isActive.Value = false;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
