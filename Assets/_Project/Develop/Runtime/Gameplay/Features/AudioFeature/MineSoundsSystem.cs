using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AudioFeature
{
    // A mine's sounds: placed on the ground, then its blast.
    public class MineSoundsSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly IAudioService _audio;

        private IDisposable _isDeadDisposable;

        public MineSoundsSystem(IAudioService audio)
        {
            _audio = audio;
        }

        public void OnInit(Entity entity)
        {
            _audio.Play(SoundIds.MinePlace);
            _isDeadDisposable = entity.IsDead.Subscribe(OnIsDeadChanged);
        }

        public void OnDispose()
        {
            _isDeadDisposable.Dispose();
        }

        private void OnIsDeadChanged(bool oldValue, bool isDead)
        {
            if (isDead)
                _audio.Play(SoundIds.MineBlast);
        }
    }
}
