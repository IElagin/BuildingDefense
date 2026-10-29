using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AudioFeature
{
    // A spitter's sounds: climbs out of the burrow, the orbital volley beam, spits when the acid leaves, dies.
    public class SpitterSoundsSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly IAudioService _audio;

        private IDisposable _attackDelayEndDisposable;
        private IDisposable _orbitalVolleyHitDisposable;
        private IDisposable _isDeadDisposable;

        public SpitterSoundsSystem(IAudioService audio)
        {
            _audio = audio;
        }

        public void OnInit(Entity entity)
        {
            _attackDelayEndDisposable = entity.AttackDelayEndEvent.Subscribe(OnAttackDelayEnd);
            _orbitalVolleyHitDisposable = entity.OrbitalVolleyHitEvent.Subscribe(OnOrbitalVolleyHit);
            _isDeadDisposable = entity.IsDead.Subscribe(OnIsDeadChanged);

            if (entity.InSpawnProcess.Value)
                _audio.Play(SoundIds.BugSpawn);
        }

        public void OnDispose()
        {
            _attackDelayEndDisposable.Dispose();
            _orbitalVolleyHitDisposable.Dispose();
            _isDeadDisposable.Dispose();
        }

        private void OnAttackDelayEnd()
        {
            _audio.Play(SoundIds.SpitterSpit);
        }

        private void OnOrbitalVolleyHit()
        {
            _audio.Play(SoundIds.OrbitalVolley);
        }

        private void OnIsDeadChanged(bool oldValue, bool isDead)
        {
            if (isDead)
                _audio.Play(SoundIds.BugDeath);
        }
    }
}
