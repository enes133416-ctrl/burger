using BurgerRush.Data;
using UnityEngine;

namespace BurgerRush.Audio
{
    /// <summary>Ses sisteminin genel kontrolü (GDD 9.3).</summary>
    public class AudioManager : Utils.Singleton<AudioManager>
    {
        [SerializeField] private AudioData audioData;
        [SerializeField] private MusicController musicController;
        [SerializeField] private SFXController sfxController;

        private MusicTrack currentTrack = MusicTrack.MainMenu;

        public void Initialize()
        {
            var settings = SaveManager.Instance.Data.settings;
            if (musicController) musicController.SetVolume(settings.music ? 0.35f : 0f);
            if (sfxController) sfxController.SetVolume(settings.sfx ? 0.7f : 0f);
        }

        public void PlayMusic(MusicTrack track)
        {
            currentTrack = track;
            if (!audioData || !musicController) return;
            if (!SaveManager.Instance.Data.settings.music) return;
            musicController.Play(audioData.MusicFor(track));
        }

        public MusicTrack CurrentTrack => currentTrack;

        public void PlaySFX(string key)
        {
            if (!audioData || !sfxController) return;
            if (!SaveManager.Instance.Data.settings.sfx) return;
            sfxController.Play(audioData.SfxFor(key));
        }

        public void SetMusicEnabled(bool enabled)
        {
            SaveManager.Instance.Data.settings.music = enabled;
            SaveManager.Instance.Save();
            if (enabled) PlayMusic(currentTrack);
            else musicController?.Stop();
        }

        public void SetSfxEnabled(bool enabled)
        {
            SaveManager.Instance.Data.settings.sfx = enabled;
            SaveManager.Instance.Save();
            sfxController?.SetVolume(enabled ? 0.7f : 0f);
        }
    }
}
