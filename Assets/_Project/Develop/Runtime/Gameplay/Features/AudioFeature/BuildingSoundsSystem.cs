using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AudioFeature
{
    // The dome's sounds: every hit and its destruction.
    public class BuildingSoundsSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly IAudioService _audio;

        private IDisposable _takeDamageDisposable;
        private IDisposable _isDeadDisposable;

        public BuildingSoundsSystem(IAudioService audio)
        {
            _audio = audio;
        }

        public void OnInit(Entity entity)
        {
            _takeDamageDisposable = entity.TakeDamageEvent.Subscribe(OnTakeDamage);
            _isDeadDisposable = entity.IsDead.Subscribe(OnIsDeadChanged);
        }

        public void OnDispose()
        {
            _takeDamageDisposable.Dispose();
            _isDeadDisposable.Dispose();
        }

        private void OnTakeDamage(float damage) => _audio.Play(SoundIds.DomeHit);

        private void OnIsDeadChanged(bool oldValue, bool isDead)
        {
            if (isDead)
                _audio.Play(SoundIds.DomeDestroyed);
        }
    }
}
