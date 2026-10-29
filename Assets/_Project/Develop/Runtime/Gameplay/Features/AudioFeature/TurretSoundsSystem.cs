using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Audio;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AudioFeature
{
    // A turret's sounds: placed on the ground, then a shot when the bolt leaves.
    public class TurretSoundsSystem : IInitializableSystem, IDisposableSystem
    {
        private readonly IAudioService _audio;

        private IDisposable _attackDelayEndDisposable;

        public TurretSoundsSystem(IAudioService audio)
        {
            _audio = audio;
        }

        public void OnInit(Entity entity)
        {
            _audio.Play(SoundIds.TurretPlace);
            _attackDelayEndDisposable = entity.AttackDelayEndEvent.Subscribe(OnAttackDelayEnd);
        }

        public void OnDispose()
        {
            _attackDelayEndDisposable.Dispose();
        }

        private void OnAttackDelayEnd()
        {
            _audio.Play(SoundIds.TurretShot);
        }
    }
}
