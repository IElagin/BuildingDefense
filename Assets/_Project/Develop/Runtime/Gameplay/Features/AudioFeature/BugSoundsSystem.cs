using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using Assets._Project.Develop.Runtime.Utilities.Reactive;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AudioFeature
{
    // A bug's sounds: crawling out of its burrow, the orbital volley beam, then the acid blast at the dome or its death.
    public class BugSoundsSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly IAudioService _audio;

        private ReactiveVariable<bool> _isBlasted;
        private IDisposable _orbitalVolleyHitDisposable;
        private IDisposable _isDeadDisposable;

        public BugSoundsSystem(IAudioService audio)
        {
            _audio = audio;
        }

        public void OnInit(Entity entity)
        {
            _isBlasted = entity.IsBlasted;
            _orbitalVolleyHitDisposable = entity.OrbitalVolleyHitEvent.Subscribe(OnOrbitalVolleyHit);
            _isDeadDisposable = entity.IsDead.Subscribe(OnIsDeadChanged);

            if (entity.InSpawnProcess.Value)
                _audio.Play(SoundIds.BugSpawn);
        }

        public void OnDispose()
        {
            _orbitalVolleyHitDisposable.Dispose();
            _isDeadDisposable.Dispose();
        }

        private void OnOrbitalVolleyHit() => _audio.Play(SoundIds.OrbitalVolley);

        private void OnIsDeadChanged(bool oldValue, bool isDead)
        {
            if (isDead)
                _audio.Play(_isBlasted.Value ? SoundIds.AcidBlast : SoundIds.BugDeath);
        }
    }
}
