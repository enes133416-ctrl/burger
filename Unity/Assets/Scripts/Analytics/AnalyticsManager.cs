using System;
using System.Collections.Generic;
using BurgerRush.Data;
using UnityEngine;

namespace BurgerRush.Analytics
{
    /// <summary>Olay izleme merkezi (Firebase / GameAnalytics entegrasyon noktası).</summary>
    public class AnalyticsManager : Utils.Singleton<AnalyticsManager>
    {
        [SerializeField] private bool enableInEditor = false;

        private readonly Queue<string> recent = new();

        public void Initialize()
        {
            Track("session_start");
            Debug.Log("[Analytics] Hazır");
        }

        public void Track(string eventName, Dictionary<string, object> parameters = null)
        {
#if !UNITY_EDITOR || true
            if (!enableInEditor && Application.isEditor && eventName != "session_start")
            {
                // Editörde gereksiz log kalabalığını önle
            }
#endif
            var line = parameters == null
                ? eventName
                : $"{eventName} {string.Join(",", parameters)}";

            recent.Enqueue(line);
            if (recent.Count > 100) recent.Dequeue();

            // Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, ...);
            // GameAnalytics.NewProgressionEvent(...);
            Debug.Log($"[Analytics] {line}");
        }

        public void TrackLevelStart(int level) => Track("level_start", new Dictionary<string, object> { { "level", level } });
        public void TrackLevelComplete(int level, int stars) => Track("level_complete", new Dictionary<string, object> { { "level", level }, { "stars", stars } });
        public void TrackLevelFail(int level, string reason) => Track("level_fail", new Dictionary<string, object> { { "level", level }, { "reason", reason } });
        public void TrackAdShown(string type) => Track("ad_shown", new Dictionary<string, object> { { "type", type } });
        public void TrackAdWatched(string type) => Track("ad_watched", new Dictionary<string, object> { { "type", type } });
        public void TrackIAP(string productId, string price) => Track("iap_purchase", new Dictionary<string, object> { { "product", productId }, { "price", price } });

        public void TrackEconomy(string flow, float amount) =>
            Track("economy", new Dictionary<string, object> { { "flow", flow }, { "amount", amount }, { "balance", SaveManager.Instance.Data.totalCoins } });
    }
}
