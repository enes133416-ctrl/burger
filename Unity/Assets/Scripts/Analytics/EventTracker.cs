using BurgerRush.Core;
using UnityEngine;

namespace BurgerRush.Analytics
{
    /// <summary>Oyun olaylarını analitik servisine bağlayan pasif dinleyici.</summary>
    public class EventTracker : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.Subscribe<LevelCompleteEvent>(OnLevelComplete);
            EventBus.Subscribe<LevelFailEvent>(OnLevelFail);
            EventBus.Subscribe<UpgradePurchasedEvent>(OnUpgrade);
            EventBus.Subscribe<FoodBurntEvent>(OnFoodBurnt);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LevelCompleteEvent>(OnLevelComplete);
            EventBus.Unsubscribe<LevelFailEvent>(OnLevelFail);
            EventBus.Unsubscribe<UpgradePurchasedEvent>(OnUpgrade);
            EventBus.Unsubscribe<FoodBurntEvent>(OnFoodBurnt);
        }

        private void OnLevelComplete(LevelCompleteEvent e) => AnalyticsManager.Instance.TrackLevelComplete(e.Level, e.Stars);
        private void OnLevelFail(LevelFailEvent e) => AnalyticsManager.Instance.TrackLevelFail(e.Level, e.Reason);
        private void OnUpgrade(UpgradePurchasedEvent e) => AnalyticsManager.Instance.Track("upgrade", null);
        private void OnFoodBurnt(FoodBurntEvent e) => AnalyticsManager.Instance.Track("food_burnt", null);
    }
}
