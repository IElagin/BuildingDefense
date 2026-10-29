using Assets._Project.Develop.Runtime.Configs.Audio;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities.Audio
{
    // Project-lifetime audio: a pool of one-shot sources and two ambient sources that crossfade between playlist tracks
    // in the order of the ambient's clip list.
    public class AudioService : MonoBehaviour, IAudioService
    {
        private const int OneShotSourcesCount = 12;

        private readonly List<AudioSource> _oneShotSources = new();
        private readonly Dictionary<SoundIds, float> _lastPlayTimes = new();
        private readonly Dictionary<SoundIds, (AudioSource source, AudioClip clip)> _lastSources = new();
        private readonly Dictionary<AudioChannels, float> _channelVolumes = new();

        private AudioConfig _config;
        private AudioSource[] _ambientSources;
        private int _activeAmbientSource;
        private int _nextOneShotSource;

        private AudioConfig.AmbientConfig _currentAmbient;
        private int _currentTrack;
        private float _fadeTime;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            for (int i = 0; i < OneShotSourcesCount; i++)
                _oneShotSources.Add(CreateSource(false));

            _ambientSources = new[] { CreateSource(true), CreateSource(true) };
        }

        public void Initialize(AudioConfig config)
        {
            _config = config;

            foreach (AudioChannels channel in System.Enum.GetValues(typeof(AudioChannels)))
                _channelVolumes[channel] = config.GetVolume(channel);
        }

        public void Play(SoundIds id)
        {
            AudioConfig.SoundConfig sound = _config.GetSound(id);

            if (sound.Clips.Count == 0)
                return;

            if (_lastPlayTimes.TryGetValue(id, out float lastTime) && Time.unscaledTime - lastTime < sound.Cooldown)
                return;

            _lastPlayTimes[id] = Time.unscaledTime;

            AudioSource source = _oneShotSources[_nextOneShotSource];
            _nextOneShotSource = (_nextOneShotSource + 1) % _oneShotSources.Count;

            source.clip = sound.Clips[Random.Range(0, sound.Clips.Count)];
            source.volume = sound.Volume * _channelVolumes[sound.Channel];
            source.pitch = Random.Range(sound.PitchMin, sound.PitchMax);
            source.PlayDelayed(sound.Delay);
            _lastSources[id] = (source, source.clip);
        }

        // Stops the last instance of the sound, including one still waiting for its delay.
        public void Stop(SoundIds id)
        {
            // The pooled source may already play another sound: stop it only if it still holds this one.
            if (_lastSources.Remove(id, out (AudioSource source, AudioClip clip) last) && last.source.clip == last.clip)
                last.source.Stop();
        }

        public void PlayAmbient(AmbientIds id)
        {
            AudioConfig.AmbientConfig ambient = _config.GetAmbient(id);

            if (ambient == _currentAmbient || ambient.Clips.Count == 0)
                return;

            _currentAmbient = ambient;
            _currentTrack = 0;
            CrossfadeTo(ambient.Clips[_currentTrack]);
        }

        public void SetVolume(AudioChannels channel, float volume) => _channelVolumes[channel] = Mathf.Clamp01(volume);

        private void Update()
        {
            if (_currentAmbient == null)
                return;

            _fadeTime += Time.unscaledDeltaTime;
            float fade = _config.AmbientCrossfade > 0f ? Mathf.Clamp01(_fadeTime / _config.AmbientCrossfade) : 1f;
            float volume = _currentAmbient.Volume * _channelVolumes[AudioChannels.Ambient];

            AudioSource active = _ambientSources[_activeAmbientSource];
            AudioSource previous = _ambientSources[1 - _activeAmbientSource];
            active.volume = volume * fade;
            previous.volume = previous.isPlaying ? volume * (1f - fade) : 0f;

            if (fade >= 1f && previous.isPlaying)
                previous.Stop();

            if (active.clip != null && active.time >= active.clip.length - _config.AmbientCrossfade)
                NextTrack();
        }

        private void NextTrack()
        {
            _currentTrack = (_currentTrack + 1) % _currentAmbient.Clips.Count;
            CrossfadeTo(_currentAmbient.Clips[_currentTrack]);
        }

        private void CrossfadeTo(AudioClip clip)
        {
            _activeAmbientSource = 1 - _activeAmbientSource;
            AudioSource source = _ambientSources[_activeAmbientSource];
            source.clip = clip;
            source.volume = 0f;
            source.time = 0f;
            source.Play();
            _fadeTime = 0f;
        }

        private AudioSource CreateSource(bool ambient)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.priority = ambient ? 32 : 128;
            return source;
        }
    }
}
