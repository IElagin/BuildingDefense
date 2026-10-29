using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AudioFeature
{
    // The click explosion caster's sound: the orbital strike on every served request.
    public class ClickExplosionSoundSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly IAudioService _audio;

        private IDisposable _clickExplosionDisposable;

        public ClickExplosionSoundSystem(IAudioService audio)
        {
            _audio = audio;
        }

        public void OnInit(Entity entity)
        {
            _clickExplosionDisposable = entity.ClickExplosionEvent.Subscribe(OnClickExplosion);
        }

        public void OnDispose()
        {
            _clickExplosionDisposable.Dispose();
        }

        private void OnClickExplosion(Vector3 point) => _audio.Play(SoundIds.OrbitalStrike);
    }
}
