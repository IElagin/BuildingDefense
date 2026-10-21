namespace Assets._Project.Develop.Runtime.Utilities.Audio
{
    public interface IAudioService
    {
        void Play(SoundIds id);

        void Stop(SoundIds id);

        void PlayAmbient(AmbientIds id);

        void SetVolume(AudioChannels channel, float volume);
    }
}
