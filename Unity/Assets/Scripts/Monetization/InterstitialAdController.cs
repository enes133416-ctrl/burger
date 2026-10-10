using BurgerRush.Core;
using UnityEngine;

namespace BurgerRush.Monetization
{
    /// <summary>
    /// Geçiş reklamı zamanlaması: yalnızca level tamamlanınca tetiklenir (GDD 4.2).
    /// </summary>
    public class InterstitialAdController : MonoBehaviour
    {
        private void OnEnable() => EventBus.Subscribe<LevelCompleteEvent>(OnLevelComplete);
        private void OnDisable() => EventBus.Unsubscribe<LevelCompleteEvent>(OnLevelComplete);

        private void OnLevelComplete(LevelCompleteEvent e)
            => AdManager.Instance?.TryShowInterstitial();
    }
}
