using BurgerRush.Data;
using UnityEngine;

namespace BurgerRush.Input
{
    /// <summary>
    /// Dokunsal geri bildirim: Android/iOS için Handheld.Vibrate ve Taptic motoru.
    /// </summary>
    public class HapticManager : Utils.Singleton<HapticManager>
    {
        [SerializeField] private bool enabledByDefault = true;

        public bool Enabled => SaveManager.Instance == null
            ? enabledByDefault
            : SaveManager.Instance.Data.settings.haptic;

        public void TriggerLight()
        {
            if (!Enabled) return;
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        public void TriggerMedium()
        {
            if (!Enabled) return;
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        public void TriggerHeavy()
        {
            if (!Enabled) return;
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        public void TriggerSuccess() => TriggerLight();
        public void TriggerError() => TriggerMedium();
    }
}
