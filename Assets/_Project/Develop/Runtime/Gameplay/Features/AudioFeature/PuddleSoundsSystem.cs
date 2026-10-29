using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilities.Audio;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AudioFeature
{
    // A puddle's sound: poured on the ground.
    public class PuddleSoundsSystem : IInitializableSystem
    {
        private readonly IAudioService _audio;

        public PuddleSoundsSystem(IAudioService audio)
        {
            _audio = audio;
        }

        public void OnInit(Entity entity)
        {
            _audio.Play(SoundIds.PuddlePlace);
        }
    }
}
