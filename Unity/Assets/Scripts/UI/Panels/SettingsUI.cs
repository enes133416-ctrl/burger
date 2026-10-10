using BurgerRush.Audio;
using BurgerRush.Data;
using UnityEngine;
using UnityEngine.UI;

namespace BurgerRush.UI.Panels
{
    /// <summary>14_Settings: ses, grafik, gizlilik ve kayıt sıfırlama (GDD 5.2).</summary>
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Toggle hapticToggle;
        [SerializeField] private Toggle shadowsToggle;
        [SerializeField] private Button resetButton;
        [SerializeField] private Text statsText;

        private void Awake()
        {
            if (musicToggle) musicToggle.onValueChanged.AddListener(v => AudioManager.Instance?.SetMusicEnabled(v));
            if (sfxToggle) sfxToggle.onValueChanged.AddListener(v => AudioManager.Instance?.SetSfxEnabled(v));
            if (hapticToggle) hapticToggle.onValueChanged.AddListener(v => SaveSettings(s => s.haptic = v));
            if (shadowsToggle) shadowsToggle.onValueChanged.AddListener(v => { SaveSettings(s => s.shadows = v); QualitySettings.shadows = v ? ShadowQuality.All : ShadowQuality.Disable; });
            if (resetButton) resetButton.onClick.AddListener(ResetSave);
        }

        private void OnEnable()
        {
            var settings = SaveManager.Instance.Data.settings;
            if (musicToggle) musicToggle.isOn = settings.music;
            if (sfxToggle) sfxToggle.isOn = settings.sfx;
            if (hapticToggle) hapticToggle.isOn = settings.haptic;
            if (shadowsToggle) shadowsToggle.isOn = settings.shadows;

            var stats = SaveManager.Instance.Data.stats;
            if (statsText)
                statsText.text = $"🍔 {stats.served} servis · 🔥 {stats.burnt} yanık · 😞 {stats.lost} kaçan · ⏱ {Mathf.RoundToInt(stats.playSeconds / 60f)} dk";
        }

        private void SaveSettings(System.Action<PlayerData.SettingsData> mutate)
        {
            mutate(SaveManager.Instance.Data.settings);
            SaveManager.Instance.Save();
        }

        private void ResetSave()
        {
            SaveManager.Instance.Reset();
            Debug.Log("[Settings] Kayıt sıfırlandı");
        }
    }
}
