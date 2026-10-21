using Assets._Project.Develop.Runtime.Utilities.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Configs.Audio
{
    // All game audio in one place: clips, per-sound volume, pitch spread, cooldown and delay, ambient playlists, channel volumes.
    [CreateAssetMenu(menuName = "Configs/Audio/NewAudioConfig", fileName = "AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        [SerializeField] private List<SoundConfig> _sounds;
        [SerializeField] private List<AmbientConfig> _ambients;

        [field: SerializeField, Range(0f, 1f)] public float UIVolume { get; private set; } = 1f;
        [field: SerializeField, Range(0f, 1f)] public float GameplayVolume { get; private set; } = 1f;
        [field: SerializeField, Range(0f, 1f)] public float AmbientVolume { get; private set; } = 0.6f;
        [field: SerializeField, Min(0f)] public float AmbientCrossfade { get; private set; } = 2f;

        public SoundConfig GetSound(SoundIds id) => _sounds.First(sound => sound.Id == id);

        public AmbientConfig GetAmbient(AmbientIds id) => _ambients.First(ambient => ambient.Id == id);

        public float GetVolume(AudioChannels channel) => channel switch
        {
            AudioChannels.UI => UIVolume,
            AudioChannels.Gameplay => GameplayVolume,
            AudioChannels.Ambient => AmbientVolume,
            _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, null)
        };

        [Serializable]
        public class SoundConfig
        {
            [field: SerializeField] public SoundIds Id { get; private set; }
            [field: SerializeField] public AudioChannels Channel { get; private set; }
            [field: SerializeField] public List<AudioClip> Clips { get; private set; }
            [field: SerializeField, Range(0f, 1f)] public float Volume { get; private set; } = 1f;
            [field: SerializeField, Range(0.5f, 2f)] public float PitchMin { get; private set; } = 1f;
            [field: SerializeField, Range(0.5f, 2f)] public float PitchMax { get; private set; } = 1f;
            [field: SerializeField, Min(0f)] public float Cooldown { get; private set; }
            [field: SerializeField, Min(0f)] public float Delay { get; private set; }
        }

        [Serializable]
        public class AmbientConfig
        {
            [field: SerializeField] public AmbientIds Id { get; private set; }
            [field: SerializeField] public List<AudioClip> Clips { get; private set; }
            [field: SerializeField, Range(0f, 1f)] public float Volume { get; private set; } = 1f;
        }
    }
}
