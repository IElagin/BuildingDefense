using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AudioFeature
{
    // An acid spit splashes when it bursts.
    public class AcidSpitSoundsSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly IAudioService _audio;

        private IDisposable _isBlastedDisposable;

        public AcidSpitSoundsSystem(IAudioService audio)
        {
            _audio = audio;
        }

        public void OnInit(Entity entity)
        {
            _isBlastedDisposable = entity.IsBlasted.Subscribe(OnIsBlastedChanged);
        }

        public void OnDispose()
        {
            _isBlastedDisposable.Dispose();
        }

        private void OnIsBlastedChanged(bool oldValue, bool isBlasted)
        {
            if (isBlasted)
                _audio.Play(SoundIds.AcidSpitSplash);
        }
    }
}
